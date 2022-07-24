using STUN.Enums;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class ProxySettingViewModel : BaseViewModel
    {
        private bool _isSocks5Proxy;
        private string _proxyServer = "127.0.0.1:1080";
        private string _proxyUsername;
        private string _proxyPassword;
        private ProxyType _proxyType = ProxyType.Plain;

        public bool IsSocks5Proxy
        {
            get { return _isSocks5Proxy; }
            set { SetProperty(ref _isSocks5Proxy, value); }
        }

        public string ProxyServer
        {
            get { return _proxyServer; }
            set { SetProperty(ref _proxyServer, value); }
        }

        public string ProxyUsername
        {
            get { return _proxyUsername; }
            set { SetProperty(ref _proxyUsername, value); }
        }

        public string ProxyPassword
        {
            get { return _proxyPassword; }
            set { SetProperty(ref _proxyPassword, value); }
        }

        public ProxyType ProxyType
        {
            get { return _proxyType; }
            set { SetProperty(ref _proxyType, value); }
        }
    }
}
