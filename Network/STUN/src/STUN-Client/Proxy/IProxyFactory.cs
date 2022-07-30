namespace STUN.Proxy
{
    public interface IUdpProxyFactory
    {
        Task<IUdpProxy> CreateProxyAsync(UdpProxyCreateOption createOption);
    }
}
