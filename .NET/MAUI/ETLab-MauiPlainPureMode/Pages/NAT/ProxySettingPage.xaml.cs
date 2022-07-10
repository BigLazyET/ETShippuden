namespace ETLab_MauiPlainPureMode.Pages;

public partial class ProxySettingPage : ContentPage
{
	public ProxySettingPage()
	{
		InitializeComponent();

		BindingContext = App.ProxySettingViewModel;
	}

	private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if(e is not null && e.Value && sender is RadioButton radioBtn && radioBtn is not null)
		{
			var value = radioBtn.Value.ToString();
			App.ProxySettingViewModel.IsSocks5Proxy = value == "Socks5";
		}
	}
}