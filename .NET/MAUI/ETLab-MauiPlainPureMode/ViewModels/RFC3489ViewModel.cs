using STUN;
using System.Windows.Input;
using ETLab_MauiPlainPureMode.Models;
using Socks5.Models;
using Dns.Net.Abstractions;
using Dns.Net.Clients;
using System.Net;

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

        public Command<CancellationToken> CheckNATTypeCommand { get; private set; }

        public RFC3489ViewModel()
        {
            CheckNATTypeCommand = new Command<CancellationToken>(CheckNATType);
        }

        private async void CheckNATType(CancellationToken token)
        {
            if (!StunServer.TryParse(Config.StunServer, out StunServer? sever))
                throw new InvalidOperationException("WRONG STUN Server");

            if (!HostnameEndpoint.TryParse(Config.ProxyServer, out HostnameEndpoint? proxyIpe))
                throw new NotSupportedException("Unknown proxy address");

            Socks5CreateOption sock5Option = new()
            {
                Address = await DnsClient.QueryAsync(proxyIpe.Hostname, token),
                Port = proxyIpe.Port,
                UsernamePassword = new UsernamePassword { UserName = Config.ProxyUser, Password = Config.ProxyPassword }
            };

            IPAddress? serverIp;
        }
    }
}
