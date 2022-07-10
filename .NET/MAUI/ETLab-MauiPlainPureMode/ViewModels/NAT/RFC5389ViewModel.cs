using Dns.Net.Abstractions;
using Dns.Net.Clients;
using ETLab_MauiPlainPureMode.Models;
using Microsoft;
using Socks5.Models;
using STUN.Client;
using STUN.Proxy;
using STUN;
using System.Net.Sockets;
using System.Net;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC5389ViewModel : BaseViewModel
    {
        const ushort DefaultStunServerPort = 3478;

        private IDnsClient _defaultDnsClient = new DefaultDnsClient();
        private IDnsClient _defaultADnsClient = new DefaultAClient();
        private IDnsClient _defaultAAAADnsClient = new DefaultAAAAClient();
        private ProxySettingViewModel _proxySetting = App.ProxySettingViewModel;

        public IEnumerable<string> STUNServers => Constants.STUNServers;

        public NATCheck5389Outcome NATCheck5389Outcome { get; set; }

        public Command CheckNATTypeCommand { get; private set; }

        public string SelectedStunServer { get; set; } = @"stun.miwifi.com";

        public RFC5389ViewModel()
        {
            NATCheck5389Outcome = new NATCheck5389Outcome();
            CheckNATTypeCommand = new Command(CheckNATType);
        }

        private async void CheckNATType()
        {
            if (string.IsNullOrEmpty(SelectedStunServer))
            {
                await Application.Current.MainPage.DisplayAlert("警告", "请选择STUN Server", "好的");
                return;
            }

            var cancellationToken = new CancellationTokenSource().Token;

            var proxyServer = _proxySetting.ProxyServer;
            var proxyUsername = _proxySetting.ProxyUsername;
            var proxyPassword = _proxySetting.ProxyPassword;
            var proxyType = _proxySetting.ProxyType;

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
            if (NATCheck5389Outcome.LocalIPEndPoint is null)
            {
                stunServerIp = await _defaultDnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
                NATCheck5389Outcome.LocalIPEndPoint = stunServerIp.AddressFamily is AddressFamily.InterNetworkV6 ?
                    new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) : new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            }
            else
            {
                if (NATCheck5389Outcome.LocalIPEndPoint.AddressFamily is AddressFamily.InterNetworkV6)
                    stunServerIp = await _defaultAAAADnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
                else
                    stunServerIp = await _defaultADnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
            }

            using var udpProxy = ProxyFactory.CreateProxy(proxyType, NATCheck5389Outcome.LocalIPEndPoint, sock5Option);
            using var stunClient5389 = new StunClient5389(new IPEndPoint(stunServerIp, stunHostNameEndPoint.Port), NATCheck5389Outcome.LocalIPEndPoint, udpProxy);

            //NATCheck3489Outcome.NATTYPE = stunClient3489.ClassicStunResult.NATType;
            //NATCheck3489Outcome.LocalIPEndPoint = stunClient3489.ClassicStunResult.LocalEndPoint;
            //NATCheck3489Outcome.PublicIPEndPoint = stunClient3489.ClassicStunResult.PublicEndPoint;

            await stunClient5389.ConnectProxyAsync(cancellationToken);
            try
            {
                await stunClient5389.QueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"{ex.Message}", "好的");
            }
            finally
            {
                await stunClient5389.CloseProxyAsync(cancellationToken);
            }

            //NATCheck3489Outcome.NATTYPE = stunClient3489.ClassicStunResult.NATType;
            //NATCheck3489Outcome.LocalIPEndPoint = stunClient3489.ClassicStunResult.LocalEndPoint;
            //NATCheck3489Outcome.PublicIPEndPoint = stunClient3489.ClassicStunResult.PublicEndPoint;
        }
    }
}
