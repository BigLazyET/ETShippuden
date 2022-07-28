using Dns.Net.Clients;
using Microsoft;
using Socks5.Models;
using STUN.Client;
using STUN.Enums;
using STUN.Proxy;

namespace STUN.Extensions
{
    public class StunClientFactory : IStunClientFactory
    {
        private readonly IProxyFactory _proxyFactory;
        private readonly DefaultDnsClient _defaultDnsClient;
        private readonly DefaultAClient _defaultAClient;
        private readonly DefaultAAAAClient _defaultAAAAClient;

        public StunClientFactory(IProxyFactory proxyFactory, DefaultDnsClient defaultDnsClient, DefaultAClient defaultAClient, DefaultAAAAClient defaultAAAAClient)
        {
            _proxyFactory = proxyFactory;
        }

        public async Task<IStunClient> GetClient(ProxyType proxyType, string proxyServer, string stunServer, string proxyUser, string proxyPwd)
        {
            Requires.NotNullOrEmpty(proxyServer, nameof(proxyServer));
            Requires.NotNullOrEmpty(stunServer, nameof(stunServer));

            Verify.Operation(HostNameEndPoint.TryParse(proxyServer, out var proxyHostnameEndPoint), "Unknow proxy address");
            var proxyAddress = await _defaultDnsClient.QueryAsync(proxyHostnameEndPoint.HostName!);
            var socks5Option = new Socks5CreateOption
            {
                Address = proxyAddress,
                Port = proxyHostnameEndPoint.Port,
                UsernamePassword = new() { UserName = proxyUser, Password = proxyPwd }
            };


            return null;
        }
    }
}
