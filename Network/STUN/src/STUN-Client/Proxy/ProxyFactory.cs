using Microsoft;
using Socks5.Models;
using STUN.Enums;
using System.Net;
using System.Runtime.CompilerServices;

namespace STUN.Proxy
{
    public class ProxyFactory
    {
        public static IUdpProxy CreateProxy(ProxyType proxyType, IPEndPoint localEndPoint, Socks5CreateOption socks5CreateOption)
        {
            IUdpProxy udpProxy = proxyType switch
            {
                ProxyType.Plain => new NoneUdpProxy(localEndPoint),
                ProxyType.Socks5 => GetUdpProxy(localEndPoint, socks5CreateOption),
                _ => throw Assumes.NotReachable()
            };

            return udpProxy;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IUdpProxy GetUdpProxy(IPEndPoint localEndPoint, Socks5CreateOption socks5CreateOption)
            {
                Requires.NotNull(socks5CreateOption, nameof(socks5CreateOption));
                Requires.Argument(socks5CreateOption.Address is not null, nameof(socks5CreateOption), "Proxy Server is null");
                return new Socks5UdpProxy(localEndPoint, socks5CreateOption);
            }
        }
    }
}
