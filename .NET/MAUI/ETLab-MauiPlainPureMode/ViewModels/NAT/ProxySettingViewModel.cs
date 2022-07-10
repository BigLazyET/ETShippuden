using STUN.Enums;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class ProxySettingViewModel : BaseViewModel
    {
        public string ProxyServer { get; set; } = "127.0.0.1:1080";

        public string ProxyUsername { get; set; }

        public string ProxyPassword { get; set; }

        public ProxyType ProxyType { get; set; } = ProxyType.Plain;
    }
}
