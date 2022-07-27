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
using System.Windows.Input;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC5389ViewModel : BaseViewModel
    {
        const ushort DefaultStunServerPort = 3478;

        private IDnsClient _defaultDnsClient = new DefaultDnsClient();
        private IDnsClient _defaultADnsClient = new DefaultAClient();
        private IDnsClient _defaultAAAADnsClient = new DefaultAAAAClient();
        private ProxySettingViewModel _proxySetting = App.ProxySettingViewModel;
        private readonly IProxyFactory _proxyFactory;

        public IEnumerable<string> StunServers => Constants.StunServers;

        public NatCheck5389Outcome NatCheck5389Outcome { get; set; }

        public ICommand CheckNatTypeCommand { get; private set; }

        public string SelectedStunServer { get; set; } = Constants.StunServers.First();

        public RFC5389ViewModel()
        {
            _proxyFactory = MauiProgram.ServiceProvider.GetRequiredService<IProxyFactory>();

            NatCheck5389Outcome = new NatCheck5389Outcome();
            // CheckNATTypeCommand = new Command(async () => await CheckNATType());
            CheckNatTypeCommand = new AsyncCommand(CheckNATTypeAsync);
        }

        private async Task CheckNATTypeAsync()
        {
            if (string.IsNullOrEmpty(SelectedStunServer))
            {
                await Application.Current.MainPage.DisplayAlert("警告", "请选择STUN Server", "好的");
                return;
            }

            var cancellationToken = new CancellationTokenSource().Token;

            Verify.Operation(HostNameEndPoint.TryParse(_proxySetting.ProxyServer, out HostNameEndPoint proxyHostNameEndPoint), "Unknown proxy address");
            Socks5CreateOption sock5Option = new()
            {
                Address = await _defaultDnsClient.QueryAsync(proxyHostNameEndPoint.HostName, cancellationToken),
                Port = proxyHostNameEndPoint.Port,
                UsernamePassword = new UsernamePassword { UserName = _proxySetting.ProxyUsername, Password = _proxySetting.ProxyPassword }
            };

            Verify.Operation(HostNameEndPoint.TryParse(SelectedStunServer, out HostNameEndPoint stunHostNameEndPoint, DefaultStunServerPort), @"WRONG STUN Server");
            IPAddress stunServerIp;
            if (NatCheck5389Outcome.LocalIPEndPoint is null)
            {
                stunServerIp = await _defaultDnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
                NatCheck5389Outcome.LocalIPEndPoint = stunServerIp.AddressFamily is AddressFamily.InterNetworkV6 ?
                    new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) : new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
            }
            else
            {
                if (NatCheck5389Outcome.LocalIPEndPoint.AddressFamily is AddressFamily.InterNetworkV6)
                    stunServerIp = await _defaultAAAADnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
                else
                    stunServerIp = await _defaultADnsClient.QueryAsync(stunHostNameEndPoint.HostName, cancellationToken);
            }

            var proxyType = _proxySetting.ProxyType;
            using var udpProxy = _proxyFactory.CreateProxy(proxyType, NatCheck5389Outcome.LocalIPEndPoint, sock5Option);
            using var stunClient5389 = new StunClient5389(new IPEndPoint(stunServerIp, stunHostNameEndPoint.Port), udpProxy, TimeSpan.FromSeconds(3));

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

            NatCheck5389Outcome.NatType = stunClient5389.StunResult5389.NATType;
            NatCheck5389Outcome.LocalIPEndPoint = stunClient5389.StunResult5389.LocalEndPoint;
            NatCheck5389Outcome.PublicIPEndPoint = stunClient5389.StunResult5389.PublicEndPoint;
            NatCheck5389Outcome.BindingTest = stunClient5389.StunResult5389.BindingTestResult;
            NatCheck5389Outcome.MappingBehavior = stunClient5389.StunResult5389.MappingBehavior;
            NatCheck5389Outcome.FilteringBehavior = stunClient5389.StunResult5389.FilteringBehavior;
        }
    }
}
