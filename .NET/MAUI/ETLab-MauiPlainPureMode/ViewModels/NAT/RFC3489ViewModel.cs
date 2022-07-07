using Dns.Net.Abstractions;
using Dns.Net.Clients;
using ETLab_MauiPlainPureMode.Models;
using Microsoft;
using Socks5.Models;
using STUN;
using STUN.Client;
using STUN.Enums;
using STUN.Proxy;
using System.Net;
using System.Net.Sockets;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC3489ViewModel : BaseViewModel
    {
        private IDnsClient _defaultDnsClient = new DefaultDnsClient();
        private IDnsClient _defaultADnsClient = new DefaultAClient();
        private IDnsClient _defaultAAAADnsClient = new DefaultAAAAClient();

        public IEnumerable<string> STUNServers => Constants.STUNServers;

        public NATCheck3489Outcome NATCheck3489Outcome => new NATCheck3489Outcome();

        public Command CheckNATTypeCommand { get; private set; }

        public RFC3489ViewModel()
        {
            CheckNATTypeCommand = new Command(CheckNATType);
        }

        private async void CheckNATType()
        {
            var stunServerAdress = Preferences.Get("StunServer", "stun.syncthing.net");
            var proxyUser = Preferences.Get("ProxyUser", String.Empty);
            var proxyPassword = Preferences.Get("ProxyPassword", String.Empty);
            var proxyType = Enum.Parse<ProxyType>(Preferences.Get("ProxyType", ProxyType.Plain.ToString()));

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
#nullable enable
            Verify.Operation(StunServer.TryParse(stunServerAdress, out StunServer? stunServer), @"WRONG STUN Server");
#nullable disable
            if (!HostNameEndPoint.TryParse(stunServerAdress, out HostNameEndPoint? proxyIp))
                throw new NotSupportedException("Unknown proxy address");

            Socks5CreateOption sock5Option = new()
            {
                Address = await _defaultDnsClient.QueryAsync(proxyIp.Hostname, cancellationToken),
                Port = proxyIp.Port,
                UsernamePassword = new UsernamePassword { UserName = proxyUser, Password = proxyPassword }
            };

            IPAddress? stunServerIp;
            if (NATCheck3489Outcome.LocalIPEndPoint is null)
            {
                stunServerIp = await _defaultDnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
                NATCheck3489Outcome.LocalIPEndPoint = stunServerIp.AddressFamily is AddressFamily.InterNetworkV6 ?
                    new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) : new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            }
            else
            {
                if (NATCheck3489Outcome.LocalIPEndPoint.AddressFamily is AddressFamily.InterNetworkV6)
                    stunServerIp = await _defaultAAAADnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
                else
                    stunServerIp = await _defaultADnsClient.QueryAsync(stunServer.Hostname, cancellationToken);
            }

            using var udpProxy = ProxyFactory.CreateProxy(proxyType, NATCheck3489Outcome.LocalIPEndPoint, sock5Option);
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
