namespace STUN.Client
{
    public interface IStunClient : IDisposable
    {
        StunResult StunResult { get; }

        ValueTask ConnectProxyAsync(CancellationToken cancellationToken = default);

        ValueTask CloseProxyAsync(CancellationToken cancellationToken = default);

        ValueTask QueryAsync(CancellationToken cancellationToken = default);
    }
}
