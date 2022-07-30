using STUN.Messages;
using STUN.Proxy;
using System.Buffers;
using System.Net.Sockets;
using System.Net;
using Microsoft;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace STUN.Client
{
    public abstract class StunClient : IStunClient
    {
        private IPEndPoint _remoteEndPoint;

        protected IUdpProxy UdpProxy { get; set; }

        protected TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public StunResult5389 StunResult5389 => new();

        public StunClient(IPEndPoint remoteEndPoint, IUdpProxy udpProxy, TimeSpan receiveTimeout)
        {
            Requires.NotNull(remoteEndPoint, nameof(remoteEndPoint));
            Requires.NotNull(udpProxy, nameof(udpProxy));

            _remoteEndPoint = remoteEndPoint;
            UdpProxy = udpProxy;
            ReceiveTimeout = receiveTimeout;
        }

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

        public abstract ValueTask QueryAsync(CancellationToken cancellationToken = default);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasValidOtherAddress([NotNullWhen(true)] IPEndPoint? other)
        {
            return other is not null
                   && !Equals(other.Address, _remoteEndPoint.Address)
                   && other.Port != _remoteEndPoint.Port;
        }
    }
}
