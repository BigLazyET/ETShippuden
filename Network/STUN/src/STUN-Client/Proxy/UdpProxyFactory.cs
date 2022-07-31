using Dns.Net.Clients;
using Microsoft;
using Socks5.Models;
using STUN.Enums;

namespace STUN.Proxy
{
    public class UdpProxyFactory : IUdpProxyFactory
    {
        private readonly DefaultDnsClient _defaultDnsClient;

        public UdpProxyFactory(DefaultDnsClient defaultDnsClient)
        {
            _defaultDnsClient = defaultDnsClient;
        }
        public async Task<IUdpProxy> CreateProxyAsync(UdpProxyCreateOption createOption)
        {
            Requires.NotNull(createOption, nameof(createOption));
            Requires.Argument(createOption.LocalEndPoint is not null, nameof(createOption), "LocalEndPoint is null");

            if (createOption.proxyType == ProxyType.Plain)
                return new NoneUdpProxy(createOption.LocalEndPoint);

            Requires.NotNullOrEmpty(createOption.ProxyServer, nameof(createOption.ProxyServer));
            Verify.Operation(HostNameEndPoint.TryParse(createOption.ProxyServer, out var proxyHostnameEndPoint), "Unknow proxy address");
            
            var proxyIp = await _defaultDnsClient.QueryAsync(proxyHostnameEndPoint.HostName!);
            var socks5Option = new Socks5CreateOption
            {
                Address = proxyIp,
                Port = proxyHostnameEndPoint.Port,
                UsernamePassword = new() { UserName = createOption.ProxyUsername, Password = createOption.ProxyPassword }
            };

            return new Socks5UdpProxy(createOption.LocalEndPoint, socks5Option);
        }
    }
}
