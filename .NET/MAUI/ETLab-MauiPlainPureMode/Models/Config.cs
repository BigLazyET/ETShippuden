using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLab_MauiPlainPureMode.Models
{
    public class Config : ObservableObject
    {
        private string _stunServer = @"stun.syncthing.net";

        public string StunServer
        {
            get { return _stunServer; }
            set { SetProperty(ref _stunServer, value); }
        }

        private ProxyType _proxyType = ProxyType.Plain;

        public ProxyType ProxyType
        {
            get { return _proxyType; }
            set { SetProperty(ref _proxyType, value); }
        }

        private string _proxyServer = @"127.0.0.1:1080";

        public string ProxyServer
        {
            get => _proxyServer;
            set => SetProperty(ref _proxyServer, value);
        }

        private string _proxyUser;

        public string ProxyUser { get => _proxyUser; set => SetProperty(ref _proxyUser, value); }

        private string _proxyPassword;

        public string ProxyPassword
        {
            get => _proxyPassword;
            set => SetProperty(ref _proxyPassword, value);
        }
    }
}
