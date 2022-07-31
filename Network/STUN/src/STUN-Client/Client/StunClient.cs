using STUN.Messages;
using STUN.Proxy;
using System.Buffers;
using System.Net.Sockets;
using System.Net;
using Microsoft;

namespace STUN.Client
{
    public abstract class StunClient : IStunClient
    {
        protected IPEndPoint RemoteEndPoint { get; set; }

        protected IUdpProxy UdpProxy { get; set; }

        protected TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(3);

        protected IPEndPoint LocalEndPoint => UdpProxy.LocalEndPoint;

        public abstract StunResult StunResult { get; }

        public StunClient(IPEndPoint remoteEndPoint, IUdpProxy udpProxy) : this(remoteEndPoint, udpProxy, TimeSpan.FromSeconds(3)) { }

        public StunClient(IPEndPoint remoteEndPoint, IUdpProxy udpProxy, TimeSpan receiveTimeout)
        {
            Requires.NotNull(remoteEndPoint, nameof(remoteEndPoint));
            Requires.NotNull(udpProxy, nameof(udpProxy));

            RemoteEndPoint = remoteEndPoint;
            UdpProxy = udpProxy;
            ReceiveTimeout = receiveTimeout;
        }

        public abstract ValueTask QueryAsync(CancellationToken cancellationToken = default);

        public async ValueTask CloseProxyAsync(CancellationToken cancellationToken = default)
        {
            await UdpProxy.CloseAsync(cancellationToken);
        }

        public async ValueTask ConnectProxyAsync(CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ReceiveTimeout);
            await UdpProxy.ConnectAsync(cts.Token);
        }

        public void Dispose()
        {
            UdpProxy.Dispose();
        }

        protected async ValueTask<StunResponse> RequestAsync(StunMessage5389 sendMessage, IPEndPoint remote, IPEndPoint receive, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(0x10000);
                var buffer = memoryOwner.Memory;
                var length = sendMessage.WriteTo(buffer.Span);

                var bytesSendLen = await UdpProxy.SendToAsync(buffer[..length], SocketFlags.None, remote, cancellationToken);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(ReceiveTimeout);
                var receiveMessage = await UdpProxy.ReceiveMessageFromAsync(buffer, SocketFlags.None, receive, cts.Token);

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
