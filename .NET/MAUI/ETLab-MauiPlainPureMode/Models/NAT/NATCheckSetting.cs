using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheckSetting : ObservableObject
    {
        private string _stunServer = @"stun.syncthing.net";
        /// <summary>
        /// 用来测试NAT类型的STUN服务器
        /// </summary>
        public string StunServer { get => _stunServer; set => SetProperty(ref _stunServer, value); }

        private ProxyType _proxyType = ProxyType.Plain;
        /// <summary>
        /// 连接STUN服务器的方式：直连/Sock5代理
        /// </summary>
        public ProxyType ProxyType { get => _proxyType; set => SetProperty(ref _proxyType, value); }

        private string _proxyServer = @"127.0.0.1:1080";
        /// <summary>
        /// 本地Socks5代理服务器地址(IP:Port)
        /// </summary>
        public string ProxyServer { get => _proxyServer; set => SetProperty(ref _proxyServer, value); }

        private string _proxyUser;
        /// <summary>
        /// Socks5代理认证之用户名
        /// </summary>
        public string ProxyUser { get => _proxyUser; set => SetProperty(ref _proxyUser, value); }

        private string _proxyPassword;
        /// <summary>
        /// Socks5代理认证之密码
        /// </summary>
        public string ProxyPassword { get => _proxyPassword; set => SetProperty(ref _proxyPassword, value); }
    }
}
