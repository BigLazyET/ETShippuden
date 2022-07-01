using STUN.Proxy;
using System.Net;

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
        }

        public virtual ValueTask<>
    }
}
