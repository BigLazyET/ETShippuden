using Dns.Net.Abstractions;
using Dns.Net.Clients;
using ETLab_MauiPlainPureMode.Models;
using Microsoft;
using Socks5.Models;
using STUN;
using STUN.Client;
using STUN.Proxy;
using System.Net;
using System.Net.Sockets;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class NATCheckViewModel : BaseViewModel
    {
        private IDnsClient DnsClient = new DefaultDnsClient();
        private IDnsClient ADnsClient = new DefaultAClient();
        private IDnsClient AAAADnsClient = new DefaultAAAAClient();

        public IEnumerable<string> STUNServers => Constants.STUNServers;

        private bool _isSocks5Selected = false;
        public bool IsSocks5Selected { get => _isSocks5Selected; set => SetProperty(ref _isSocks5Selected, value); }

        private NATCheckSetting _natCheckSetting;
        public NATCheckSetting NATCheckSetting { get => _natCheckSetting; set => SetProperty(ref _natCheckSetting, value); }

        private NATCheck3489Outcome _check3489Outcome;
        public NATCheck3489Outcome NATCheck3489Outcome { get => _check3489Outcome; set => SetProperty(ref _check3489Outcome, value); }

        private NATCheck5780Outcome _check5780Outcome;
        public NATCheck5780Outcome NATCheck5780Outcome { get => _check5780Outcome; set => SetProperty(ref _check5780Outcome, value); }

        public Command CheckNATTypeCommand { get; private set; }

        public NATCheckViewModel()
        {
            NATCheckSetting = new NATCheckSetting();
            NATCheck3489Outcome = new NATCheck3489Outcome();
            NATCheck5780Outcome = new NATCheck5780Outcome();

            CheckNATTypeCommand = new Command(CheckNATType);
        }

        private async void CheckNATType()
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token;

            //if (!StunServer.TryParse(Config.StunServer, out StunServer? stunServer))
            //    throw new InvalidOperationException("WRONG STUN Server");
            Verify.Operation(StunServer.TryParse(NATCheckSetting.StunServer, out StunServer? stunServer), @"WRONG STUN Server");

            if (!HostnameEndpoint.TryParse(NATCheckSetting.ProxyServer, out HostnameEndpoint? proxyIp))
                throw new NotSupportedException("Unknown proxy address");

            Socks5CreateOption sock5Option = new()
            {
                Address = await DnsClient.QueryAsync(proxyIp.Hostname, cancellationToken),
                Port = proxyIp.Port,
                UsernamePassword = new UsernamePassword { UserName = NATCheckSetting.ProxyUser, Password = NATCheckSetting.ProxyPassword }
            };

            IPAddress? stunServerIp;
            if (NATCheck3489Outcome.LocalIPEndPoint is null)
            {
                stunServerIp = await DnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
                NATCheck3489Outcome.LocalIPEndPoint = stunServerIp.AddressFamily is AddressFamily.InterNetworkV6 ?
                    new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) : new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            }
            else
            {
                if (NATCheck3489Outcome.LocalIPEndPoint.AddressFamily is AddressFamily.InterNetworkV6)
                    stunServerIp = await AAAADnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
                else
                    stunServerIp = await ADnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
            }

            using var udpProxy = ProxyFactory.CreateProxy(NATCheckSetting.ProxyType, NATCheck3489Outcome.LocalIPEndPoint, sock5Option);
            using var stunClient3489 = new StunClient3489(new IPEndPoint(stunServerIp, stunServer.Port), NATCheck3489Outcome.LocalIPEndPoint, udpProxy);

            NATCheck3489Outcome.NATTYPE = stunClient3489.ClassicStunResult.NATType;
            NATCheck3489Outcome.LocalIPEndPoint = stunClient3489.ClassicStunResult.LocalEndPoint;
            NATCheck3489Outcome.PublicIPEndPoint = stunClient3489.ClassicStunResult.PublicEndPoint;

            await stunClient3489.ConnectProxyAsync(cancellationToken);
            await stunClient3489.QueryAsync(cancellationToken);
            await stunClient3489.CloseProxyAsync(cancellationToken);

            NATCheck3489Outcome.NATTYPE = stunClient3489.ClassicStunResult.NATType;
            NATCheck3489Outcome.LocalIPEndPoint = stunClient3489.ClassicStunResult.LocalEndPoint;
            NATCheck3489Outcome.PublicIPEndPoint = stunClient3489.ClassicStunResult.PublicEndPoint;
        }
    }
}
