namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class ProxySettingViewModel : BaseViewModel
    {
        public string ProxyServer { get; set; } = "127.0.0.1:1080";

        public string ProxyUsername { get; set; }

        public string ProxyPassword { get; set; }

        public string ProxyType { get; set; } = STUN.Enums.ProxyType.Plain.ToString();
    }
}
