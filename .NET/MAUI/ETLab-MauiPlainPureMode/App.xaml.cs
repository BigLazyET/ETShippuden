using ETLab_MauiPlainPureMode.ViewModels;

namespace ETLab_MauiPlainPureMode;

public partial class App : Application
{
    /// <summary>
    /// 共享设置ViewModel
    /// </summary>
    public static SettingViewModel SettingViewModel { get; private set; }

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        SettingViewModel = new SettingViewModel();
    }
}
