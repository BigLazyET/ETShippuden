using Socks5.Models;
using STUN.Enums;
using System.Net;

namespace STUN.Proxy
{
    public interface IProxyFactory
    {
        IUdpProxy CreateProxy(ProxyType proxyType, IPEndPoint localEndPoint, Socks5CreateOption socks5CreateOption)
    }
}
