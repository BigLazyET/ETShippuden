using STUN.Enums;
using STUN.Messages;
using STUN.Proxy;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace STUN.Client
{
    public class StunClient3489 : IStunClient
    {
        private readonly IUdpProxy _udpProxy;

        private readonly IPEndPoint _remoteEndPoint;

        public virtual IPEndPoint LocalEndPoint => (IPEndPoint)_udpProxy.Client.LocalEndPoint!;

        public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public ClassicStunResult ClassicStunResult { get; } = new();

        public StunClient3489(IPEndPoint remoteEndPoint, IPEndPoint localEndPoint, IUdpProxy? udpProxy = null)
        {
            this._udpProxy = udpProxy ?? new NoneUdpProxy(localEndPoint);
            _remoteEndPoint = remoteEndPoint;
            ClassicStunResult.LocalEndPoint = localEndPoint;
        }

        public async ValueTask CloseProxyAsync(CancellationToken cancellationToken = default)
        {
            await _udpProxy.CloseAsync(cancellationToken);
        }

        public async ValueTask ConnectProxyAsync(CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ReceiveTimeout);
            await _udpProxy.ConnectAsync(cts.Token);
        }

        public void Dispose()
        {
            _udpProxy.Dispose();
        }

        /// <summary>
        /// 判断NAT类型的核心逻辑
        /// 基本认知：STUN服务端部署在一台有着两个公网IP的服务器上
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask QueryAsync(CancellationToken cancellationToken = default)
        {
            ClassicStunResult.Reset();

            // Step 1：send request to server => messageType: BindingRequest; attributeType not set
            var stepResponse1 = await TestStep1Async(cancellationToken);

            // Not Receive：udp blocked
            if (stepResponse1 == null)
            {
                ClassicStunResult.NATType = NATType.UdpBlocked;
                return;
            }

            // Request IP is alse be returned by response
            ClassicStunResult.LocalEndPoint = new IPEndPoint(stepResponse1.localAddress, LocalEndPoint.Port);

            // Get the public ip of client according to the response from the server
            var mappedAddress1 = stepResponse1.stunMessage.GetIPEndPointFromMappedAddressAttribute();
            ClassicStunResult.PublicEndPoint = mappedAddress1;

            // Get the another IPEndPoint of server
            var serverAnotherIPEndpoint = stepResponse1.stunMessage.GetIPEndPointFromChangedAddressAttribute();

            // Check the server is Single-IP Server or not? if it is, then not-supported nat type test
            if (mappedAddress1 is null || serverAnotherIPEndpoint is null ||
                Equals(serverAnotherIPEndpoint.Address, stepResponse1.remote.Address) || Equals(serverAnotherIPEndpoint.Port, stepResponse1.remote.Port))
            {
                ClassicStunResult.NATType = NATType.UnsupportedServer;
                return;
            }

            // Is Public IP == link's IP? if it is, then no nat
            var isPublic = Equals(mappedAddress1.Address, stepResponse1.localAddress) && Equals(mappedAddress1.Port, LocalEndPoint.Port);

            // Step 2：send request to server => messageType: BindingRequest; attributeType：ChangeRequest
            var stepResponse2 = await TestStep2Async(serverAnotherIPEndpoint, cancellationToken);
            var mappedAddress2 = stepResponse2.stunMessage.GetIPEndPointFromMappedAddressAttribute();

            if (stepResponse2 != null)
            {
                // Check the server is Single-IP Server or not? if it is, then not-supported nat type test
                if (Equals(stepResponse2.remote.Address, stepResponse1.remote.Address) &&
                    (stepResponse2.remote.Port == stepResponse1.remote.Port))
                {
                    ClassicStunResult.NATType = NATType.UnsupportedServer;
                    return;
                }
            }

            if (isPublic)
            {
                if (stepResponse2 is null)
                {
                    ClassicStunResult.NATType = NATType.SymmetricUdpFirewall;
                    return;
                }
                else
                {
                    ClassicStunResult.NATType = NATType.OpenInternet;
                    return;
                }
            }

            if (stepResponse2 is not null)
            {
                ClassicStunResult.NATType = NATType.FullCone;
                ClassicStunResult.PublicEndPoint = mappedAddress2;
                return;
            }

            // Step 3：send request to server => messageType: BindingRequest; attributeType not set
            var stepResponse1_2 = await TestStep1_2Async(serverAnotherIPEndpoint, cancellationToken);

            if (stepResponse1_2 is null)
            {
                ClassicStunResult.NATType = NATType.Unknown;
                return;
            }

            var mappedAddress1_2 = stepResponse1_2.stunMessage.GetIPEndPointFromMappedAddressAttribute();
            if (!Equals(mappedAddress1_2, ClassicStunResult.PublicEndPoint))
            {
                ClassicStunResult.NATType = NATType.Symmetric;
                ClassicStunResult.PublicEndPoint = mappedAddress1_2;
                return;
            }

            // Step 4：send request to server => messageType: BindingRequest; attributeType：ChangedRequest with port (IP is not changed)
            var stepResponse3 = await TestStep3Async(cancellationToken);
            if (stepResponse3 is not null)
            {
                var mappedAddress3 = stepResponse3.stunMessage.GetIPEndPointFromMappedAddressAttribute();
                if (mappedAddress3 is not null &&
                    Equals(stepResponse3.remote.Address, stepResponse1.remote.Address) &&
                    stepResponse3.remote.Port != stepResponse1.remote.Port)
                {
                    ClassicStunResult.NATType = NATType.RestrictedCone;
                    ClassicStunResult.PublicEndPoint = mappedAddress3;
                }
            }

            ClassicStunResult.NATType = NATType.PortRestrictedCone;
            ClassicStunResult.PublicEndPoint = mappedAddress1_2;
        }

        /// <summary>
        /// Request echo from same address and same port
        /// the address and port is belong to the server1
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async ValueTask<StunResponse> TestStep1Async(CancellationToken cancellationToken)
        {
            var stunMessage = new StunMessage5389
            {
                Header = new StunMessageHeader
                {
                    StunMessageType = StunMessageType.BindingRequest,
                    MagicCookie = 0
                }
            };

            var response = await RequestAsync(stunMessage, _remoteEndPoint, _remoteEndPoint, cancellationToken);
            return response;
        }

        /// <summary>
        /// Request echo from different address and different port
        /// the address and port is belong to server2
        /// the server has a pair of IP:Port，we called Server1，Server2
        /// we send request to Server1 and request change ip and port；then
        /// server choose Server2 to response
        /// </summary>
        /// <param name="other"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async ValueTask<StunResponse> TestStep2Async(IPEndPoint other, CancellationToken cancellationToken)
        {
            var stunMessage = new StunMessage5389
            {
                Header = new StunMessageHeader
                {
                    StunMessageType = StunMessageType.BindingRequest,
                    MagicCookie = 0
                },
                Attributes = new[] { StunAttributeExtensions.BuildChangeRequest(true, true) }
            };

            var response = await RequestAsync(stunMessage, _remoteEndPoint, other, cancellationToken);
            return response;
        }

        /// <summary>
        /// Request echo from same address and same port
        /// the address and port is belong to the server2
        /// </summary>
        /// <param name="other"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async ValueTask<StunResponse> TestStep1_2Async(IPEndPoint other, CancellationToken cancellationToken)
        {
            var stunMessage = new StunMessage5389
            {
                Header = new StunMessageHeader
                {
                    StunMessageType = StunMessageType.BindingRequest,
                    MagicCookie = 0
                }
            };

            var response = await RequestAsync(stunMessage, other, other, cancellationToken);
            return response;
        }

        /// <summary>
        /// Request echo from same address and different port
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async ValueTask<StunResponse> TestStep3Async(CancellationToken cancellationToken)
        {
            var stunMessage = new StunMessage5389
            {
                Header = new StunMessageHeader
                {
                    StunMessageType = StunMessageType.BindingRequest,
                    MagicCookie = 0
                },
                Attributes = new[] { StunAttributeExtensions.BuildChangeRequest(false, true) }
            };

            var response = await RequestAsync(stunMessage, _remoteEndPoint, _remoteEndPoint, cancellationToken);
            return response;
        }

        private async ValueTask<StunResponse> RequestAsync(StunMessage5389 sendMessage, IPEndPoint remote, IPEndPoint receive, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(0x10000);
                var buffer = memoryOwner.Memory;
                var length = sendMessage.WriteTo(buffer.Span);

                await _udpProxy.SendToAsync(buffer[..length], SocketFlags.None, remote, cancellationToken);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(ReceiveTimeout);
                var receiveMessage = await _udpProxy.ReceiveMessageFromAsync(buffer, SocketFlags.None, receive, cts.Token);

                var stuMessage = new StunMessage5389();
                if (stuMessage.TryParse(buffer.Span[..receiveMessage.ReceivedBytes]) && stuMessage.IsSameTransaction(sendMessage))
                {
                    return new StunResponse(stuMessage, (IPEndPoint)receiveMessage.RemoteEndPoint, receiveMessage.PacketInformation.Address);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return default;
        }
    }
}
