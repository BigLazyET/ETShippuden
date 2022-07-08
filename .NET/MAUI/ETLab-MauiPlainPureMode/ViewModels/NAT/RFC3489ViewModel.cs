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
        const ushort DefaultStunServerPort = 3478;

        private IDnsClient _defaultDnsClient = new DefaultDnsClient();
        private IDnsClient _defaultADnsClient = new DefaultAClient();
        private IDnsClient _defaultAAAADnsClient = new DefaultAAAAClient();
        private ProxySettingViewModel _proxySetting = App.ProxySettingViewModel;

        public IEnumerable<string> STUNServers => Constants.STUNServers;

        public NATCheck3489Outcome NATCheck3489Outcome => new NATCheck3489Outcome();

        public Command CheckNATTypeCommand { get; private set; }

        public string SelectedStunServer { get; set; } = @"stun.miwifi.com";

        public RFC3489ViewModel()
        {
            CheckNATTypeCommand = new Command(CheckNATType);
        }

        private async void CheckNATType()
        {
            var proxyServer = _proxySetting.ProxyServer;
            var proxyUsername = _proxySetting.ProxyUsername;
            var proxyPassword = _proxySetting.ProxyPassword;
            var proxyType = Enum.Parse<ProxyType>(_proxySetting.ProxyType);

            var cancellationToken = new CancellationToken(false);

            Verify.Operation(HostNameEndPoint.TryParse(proxyServer, out HostNameEndPoint? proxyHostNameEndPoint), "Unknown proxy address");
            Socks5CreateOption sock5Option = new()
            {
                Address = await _defaultDnsClient.QueryAsync(proxyHostNameEndPoint.HostName, cancellationToken),
                Port = proxyHostNameEndPoint.Port,
                UsernamePassword = new UsernamePassword { UserName = proxyUsername, Password = proxyPassword }
            };

#nullable enable
            Verify.Operation(HostNameEndPoint.TryParse(SelectedStunServer, out HostNameEndPoint? stunHostNameEndPoint, DefaultStunServerPort), @"WRONG STUN Server");
#nullable disable
            IPAddress stunServerIp;
            if (NATCheck3489Outcome.LocalIPEndPoint is null)
            {
                stunServerIp = await _defaultDnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
                NATCheck3489Outcome.LocalIPEndPoint = stunServerIp.AddressFamily is AddressFamily.InterNetworkV6 ?
                    new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) : new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            }
            else
            {
                if (NATCheck3489Outcome.LocalIPEndPoint.AddressFamily is AddressFamily.InterNetworkV6)
                    stunServerIp = await _defaultAAAADnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
                else
                    stunServerIp = await _defaultADnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
            }

            using var udpProxy = ProxyFactory.CreateProxy(proxyType, NATCheck3489Outcome.LocalIPEndPoint, sock5Option);
            using var stunClient3489 = new StunClient3489(new IPEndPoint(stunServerIp, stunHostNameEndPoint.Port), NATCheck3489Outcome.LocalIPEndPoint, udpProxy);

            NATCheck3489Outcome.NATTYPE = stunClient3489.ClassicStunResult.NATType;
            NATCheck3489Outcome.LocalIPEndPoint = stunClient3489.ClassicStunResult.LocalEndPoint;
            NATCheck3489Outcome.PublicIPEndPoint = stunClient3489.ClassicStunResult.PublicEndPoint;

            await stunClient3489.ConnectProxyAsync(cancellationToken);
            try
            {
                await stunClient3489.QueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"{ex.Message}", "好的");
            }
            finally
            {
                await stunClient3489.CloseProxyAsync(cancellationToken);
            }

            NATCheck3489Outcome.NATTYPE = stunClient3489.ClassicStunResult.NATType;
            NATCheck3489Outcome.LocalIPEndPoint = stunClient3489.ClassicStunResult.LocalEndPoint;
            NATCheck3489Outcome.PublicIPEndPoint = stunClient3489.ClassicStunResult.PublicEndPoint;
        }
    }
}
