using ETLab_MauiPlainPureMode.Models;
using STUN;
using STUN.Client;
using STUN.Enums;
using STUN.Extensions;
using STUN.Proxy;
using System.Net;
using System.Windows.Input;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC5389ViewModel : BaseViewModel
    {
        private readonly IUdpProxyFactory _udpProxyFactory;
        private readonly IStunClientFactory _stunClientFactory;
        private ProxySettingViewModel _proxySetting = App.ProxySettingViewModel;

        public IEnumerable<string> StunServers => Constants.StunServers;

        public IEnumerable<IPEndPoint> LocalEndPoints => Constants.LocalEndPoints;

        public NatCheck5389Outcome NatCheck5389Outcome { get; set; }

        public ICommand CheckNatTypeCommand { get; private set; }

        public string SelectedStunServer { get; set; } = Constants.StunServers.First();

        public IPEndPoint SelectedLocalEndPoint { get; set; } = Constants.LocalEndPoints.First();

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
                LocalEndPoint = NatCheck5389Outcome.ActualLocalIPEndPoint ?? SelectedLocalEndPoint,
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

            var stunResult5389 = (StunResult5389)stunClient.StunResult;
            NatCheck5389Outcome.NatType = stunResult5389.NATType;
            NatCheck5389Outcome.LocalIPEndPoint = stunResult5389.ActualLocalEndPoint;
            NatCheck5389Outcome.PublicIPEndPoint = stunResult5389.PublicEndPoint;
            NatCheck5389Outcome.BindingTest = stunResult5389.BindingTestResult;
            NatCheck5389Outcome.MappingBehavior = stunResult5389.MappingBehavior;
            NatCheck5389Outcome.FilteringBehavior = stunResult5389.FilteringBehavior;

            // udpProxy.Dispose()
            stunClient.Dispose();
        }
    }
}
