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

        public ValueTask QueryAsync(CancellationToken cancellationToken = default)
        {
            ClassicStunResult.Reset();

            return ValueTask.CompletedTask;
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
