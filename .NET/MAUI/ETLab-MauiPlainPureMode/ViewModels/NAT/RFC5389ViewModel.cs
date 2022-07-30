using ETLab_MauiPlainPureMode.Models;
using STUN.Client;
using STUN.Enums;
using STUN.Extensions;
using STUN.Proxy;
using System.Windows.Input;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC5389ViewModel : BaseViewModel
    {
        private readonly IUdpProxyFactory _udpProxyFactory;
        private readonly IStunClientFactory _stunClientFactory;
        private ProxySettingViewModel _proxySetting = App.ProxySettingViewModel;

        public IEnumerable<string> StunServers => Constants.StunServers;

        public NatCheck5389Outcome NatCheck5389Outcome { get; set; }

        public ICommand CheckNatTypeCommand { get; private set; }

        public string SelectedStunServer { get; set; } = Constants.StunServers.First();

        public RFC5389ViewModel()
        {
            _udpProxyFactory = MauiProgram.ServiceProvider.GetRequiredService<IUdpProxyFactory>();
            _stunClientFactory = MauiProgram.ServiceProvider.GetRequiredService<IStunClientFactory>();

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

            var udpCreateOption = new UdpProxyCreateOption
            {
                LocalEndPoint = NatCheck5389Outcome.LocalIPEndPoint,
                proxyType = _proxySetting.ProxyType,
                ProxyServer = _proxySetting.ProxyServer,
                ProxyUsername = _proxySetting.ProxyUsername,
                ProxyPassword = _proxySetting.ProxyPassword
            };
            var udpProxy = await _udpProxyFactory.CreateProxyAsync(udpCreateOption);

            var stunCreateOption = new StunClientCreateOption
            {
                UdpProxy = udpProxy,
                StunProtocol = StunProtocolType.RFC5389,
                StunServer = SelectedStunServer
            };
            var stunClient = await _stunClientFactory.CreateClientAsync(stunCreateOption);

            await stunClient.ConnectProxyAsync(cancellationToken);
            try
            {
                await stunClient.QueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"{ex.Message}", "好的");
            }
            finally
            {
                await stunClient.CloseProxyAsync(cancellationToken);
            }

            NatCheck5389Outcome.NatType = stunClient.StunResult5389.NATType;
            NatCheck5389Outcome.LocalIPEndPoint = stunClient.StunResult5389.LocalEndPoint;
            NatCheck5389Outcome.PublicIPEndPoint = stunClient.StunResult5389.PublicEndPoint;
            NatCheck5389Outcome.BindingTest = stunClient.StunResult5389.BindingTestResult;
            NatCheck5389Outcome.MappingBehavior = stunClient.StunResult5389.MappingBehavior;
            NatCheck5389Outcome.FilteringBehavior = stunClient.StunResult5389.FilteringBehavior;

            // udpProxy.Dispose()
            stunClient.Dispose();
        }
    }
}
