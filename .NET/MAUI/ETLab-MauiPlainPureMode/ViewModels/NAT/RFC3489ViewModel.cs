using ETLab_MauiPlainPureMode.Models;
using STUN.Client;
using STUN.Enums;
using STUN.Extensions;
using STUN.Proxy;
using System.Net;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC3489ViewModel : BaseViewModel
    {
        private readonly IUdpProxyFactory _udpProxyFactory;
        private readonly IStunClientFactory _stunClientFactory;
        private ProxySettingViewModel _proxySetting = App.ProxySettingViewModel;

        public IEnumerable<string> STUNServers => Constants.StunServers;

        public IEnumerable<IPEndPoint> LocalEndPoints => Constants.LocalEndPoints;

        public NATCheck3489Outcome NATCheck3489Outcome { get; set; }

        public Command CheckNATTypeCommand { get; private set; }

        public string SelectedStunServer { get; set; } = Constants.StunServers.First();

        public IPEndPoint SelectedLocalEndPoint { get; set; } = Constants.LocalEndPoints.First();

        public RFC3489ViewModel()
        {
            _udpProxyFactory = MauiProgram.ServiceProvider.GetRequiredService<IUdpProxyFactory>();
            _stunClientFactory = MauiProgram.ServiceProvider.GetRequiredService<IStunClientFactory>();

            NATCheck3489Outcome = new NATCheck3489Outcome();
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

            var udpCreateOption = new UdpProxyCreateOption
            {
                LocalEndPoint = NATCheck3489Outcome.ActualLocalIPEndPoint ?? SelectedLocalEndPoint,
                proxyType = _proxySetting.ProxyType,
                ProxyServer = _proxySetting.ProxyServer,
                ProxyUsername = _proxySetting.ProxyUsername,
                ProxyPassword = _proxySetting.ProxyPassword
            };
            var udpProxy = await _udpProxyFactory.CreateProxyAsync(udpCreateOption);

            var stunCreateOption = new StunClientCreateOption
            {
                UdpProxy = udpProxy,
                StunProtocol = StunProtocolType.RFC3489,
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

            NATCheck3489Outcome.NATTYPE = stunClient.StunResult.NATType;
            NATCheck3489Outcome.LocalIPEndPoint = stunClient.StunResult.ActualLocalEndPoint;
            NATCheck3489Outcome.PublicIPEndPoint = stunClient.StunResult.PublicEndPoint;
        }
    }
}
