namespace ETLab_MauiPlainPureMode.Pages;

public partial class ProxySettingPage : ContentPage
{
	public ProxySettingPage()
	{
		InitializeComponent();

		BindingContext = App.ProxySettingViewModel;
	}
}