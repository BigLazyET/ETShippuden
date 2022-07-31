using STUN.Enums;
using STUN.Messages;
using STUN.Proxy;
using System.Net;

namespace STUN.Client
{
    public class StunClient3489 : StunClient
    {
        public override StunResult3489 StunResult => new StunResult3489();

        public StunClient3489(IPEndPoint remoteEndPoint, IUdpProxy udpProxy) : base(remoteEndPoint, udpProxy) { }

        /// <summary>
        /// 判断NAT类型的核心逻辑
        /// 基本认知：STUN服务端部署在一台有着两个公网IP的服务器上
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async ValueTask QueryAsync(CancellationToken cancellationToken = default)
        {
            StunResult.Reset();

            // Step 1：send request to server => messageType: BindingRequest; attributeType not set
            var stepResponse1 = await TestStep1Async(cancellationToken);

            // Not Receive：udp blocked
            if (stepResponse1 == null)
            {
                StunResult.NATType = NatType.UdpBlocked;
                return;
            }

            // Request IP is alse be returned by response
            StunResult.ActualLocalEndPoint = new IPEndPoint(stepResponse1.localAddress, LocalEndPoint.Port);

            // Get the public ip of client according to the response from the server
            var mappedIPEndPoint1 = stepResponse1.stunMessage.GetIPEndPointFromMappedAddressAttribute();
            StunResult.PublicEndPoint = mappedIPEndPoint1;

            // Get the another IPEndPoint of server
            var serverAnotherIPEndpoint = stepResponse1.stunMessage.GetIPEndPointFromChangedAddressAttribute();

            // Check the server is Single-IP Server or not? if it is, then not-supported nat type test
            if (mappedIPEndPoint1 is null || serverAnotherIPEndpoint is null ||
                Equals(serverAnotherIPEndpoint.Address, stepResponse1.remote.Address) || Equals(serverAnotherIPEndpoint.Port, stepResponse1.remote.Port))
            {
                StunResult.NATType = NatType.UnsupportedServer;
                return;
            }

            // Is Public IP == link's IP? if it is, then no nat
            var isPublic = Equals(mappedIPEndPoint1.Address, stepResponse1.localAddress) && Equals(mappedIPEndPoint1.Port, LocalEndPoint.Port);

            // Step 2：send request to server => messageType: BindingRequest; attributeType：ChangeRequest IP & Port
            var stepResponse2 = await TestStep2Async(serverAnotherIPEndpoint, cancellationToken);
            var mappedIPEndPoint2 = stepResponse2?.stunMessage.GetIPEndPointFromMappedAddressAttribute();

            if (isPublic)
            {
                if (stepResponse2 is null)
                {
                    StunResult.NATType = NatType.SymmetricUdpFirewall;
                    return;
                }
                else
                {
                    StunResult.NATType = NatType.OpenInternet;
                    return;
                }
            }

            if (stepResponse2 is not null)
            {
                StunResult.NATType = NatType.FullCone;
                StunResult.PublicEndPoint = mappedIPEndPoint2;
                StunResult.ActualLocalEndPoint = new IPEndPoint(stepResponse2.localAddress, LocalEndPoint.Port);
                return;
            }

            // Step 3：send request to server => messageType: BindingRequest; attributeType not set
            var stepResponse3 = await TestStep3Async(serverAnotherIPEndpoint, cancellationToken);

            if (stepResponse3 is null)
            {
                StunResult.NATType = NatType.Unknown;
                return;
            }

            var mappedIPEndPoint3 = stepResponse3.stunMessage.GetIPEndPointFromMappedAddressAttribute();
            if (!Equals(mappedIPEndPoint3, StunResult.PublicEndPoint))
            {
                StunResult.NATType = NatType.Symmetric;
                StunResult.PublicEndPoint = mappedIPEndPoint3;
                StunResult.ActualLocalEndPoint = new IPEndPoint(stepResponse3.localAddress, LocalEndPoint.Port);
                return;
            }

            // Step 4：send request to server => messageType: BindingRequest; attributeType：ChangedRequest with port (IP is not changed)
            var stepResponse4 = await TestStep4Async(cancellationToken);
            if (stepResponse4 is null)
            {
                StunResult.NATType = NatType.PortRestrictedCone;
                StunResult.PublicEndPoint = mappedIPEndPoint3;
            }

            var mappedIPEndPoint4 = stepResponse4.stunMessage.GetIPEndPointFromMappedAddressAttribute();
            if (mappedIPEndPoint4 is not null &&
                Equals(stepResponse4.remote.Address, stepResponse1.remote.Address) &&
                stepResponse4.remote.Port != stepResponse1.remote.Port)
            {
                StunResult.NATType = NatType.RestrictedCone;
                StunResult.PublicEndPoint = mappedIPEndPoint4;
                StunResult.ActualLocalEndPoint = new IPEndPoint(stepResponse4.localAddress, LocalEndPoint.Port);
            }
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

            var response = await RequestAsync(stunMessage, RemoteEndPoint, RemoteEndPoint, cancellationToken);
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

            var response = await RequestAsync(stunMessage, RemoteEndPoint, other, cancellationToken);
            return response;
        }

        /// <summary>
        /// Request echo from same address and same port
        /// the address and port is belong to the server2
        /// </summary>
        /// <param name="other"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async ValueTask<StunResponse> TestStep3Async(IPEndPoint other, CancellationToken cancellationToken)
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
        public virtual async ValueTask<StunResponse> TestStep4Async(CancellationToken cancellationToken)
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

            var response = await RequestAsync(stunMessage, RemoteEndPoint, RemoteEndPoint, cancellationToken);
            return response;
        }
    }
}
