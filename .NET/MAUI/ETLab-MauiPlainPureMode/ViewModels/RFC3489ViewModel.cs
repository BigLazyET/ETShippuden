using Dns.Net.Abstractions;
using Dns.Net.Clients;
using ETLab_MauiPlainPureMode.Models;
using Microsoft;
using Socks5.Models;
using STUN;
using STUN.Proxy;
using System.Net;
using System.Net.Sockets;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC3489ViewModel : BaseViewModel
    {
        private IDnsClient DnsClient = new DefaultDnsClient();
        private IDnsClient ADnsClient = new DefaultAClient();
        private IDnsClient AAAADnsClient = new DefaultAAAAClient();

        public IEnumerable<string> STUNServers => new List<string>
        {
            @"stun.syncthing.net",
            @"stun.qq.com",
            @"stun.miwifi.com",
            @"stun.bige0.com",
            @"stun.stunprotocol.org"
        };

        public Config Config { get; set; }

        public ClassicStunResult Result3489 { get; set; }

        public Command<CancellationToken> CheckNATTypeCommand { get; private set; }

        public RFC3489ViewModel()
        {
            Result3489 = new ClassicStunResult();
            CheckNATTypeCommand = new Command<CancellationToken>(CheckNATType);
        }

        private async void CheckNATType(CancellationToken cancellationToken)
        {
            //if (!StunServer.TryParse(Config.StunServer, out StunServer? stunServer))
            //    throw new InvalidOperationException("WRONG STUN Server");
            Verify.Operation(StunServer.TryParse(Config.StunServer, out StunServer? stunServer), @"WRONG STUN Server");

            if (!HostnameEndpoint.TryParse(Config.ProxyServer, out HostnameEndpoint? proxyIp))
                throw new NotSupportedException("Unknown proxy address");

            Socks5CreateOption sock5Option = new()
            {
                Address = await DnsClient.QueryAsync(proxyIp.Hostname, cancellationToken),
                Port = proxyIp.Port,
                UsernamePassword = new UsernamePassword { UserName = Config.ProxyUser, Password = Config.ProxyPassword }
            };

            IPAddress? stunServerIp;
            if (Result3489.LocalEndPoint is null)
            {
                stunServerIp = await DnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
                Result3489.LocalEndPoint = stunServerIp.AddressFamily is AddressFamily.InterNetworkV6 ?
                    new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) : new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            }
            else
            {
                if (Result3489.LocalEndPoint.AddressFamily is AddressFamily.InterNetworkV6)
                    stunServerIp = await AAAADnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
                else
                    stunServerIp = await ADnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
            }

            using var udpProxy = ProxyFactory.CreateProxy(Config.ProxyType, Result3489.LocalEndPoint, sock5Option);
            using var stunClient3489 = new();
        }
    }
}
