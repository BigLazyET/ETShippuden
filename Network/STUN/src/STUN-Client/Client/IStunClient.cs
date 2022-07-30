namespace STUN.Client
{
    public interface IStunClient : IDisposable
    {
        StunResult5389 StunResult5389 { get; }

        ValueTask ConnectProxyAsync(CancellationToken cancellationToken = default);

        ValueTask CloseProxyAsync(CancellationToken cancellationToken = default);

        ValueTask QueryAsync(CancellationToken cancellationToken = default);
    }
}
