using AndroidX.Lifecycle;
using ETLab_MauiPlainPureMode.Controls;
using ETLab_MauiPlainPureMode.ViewModels;

namespace ETLab_MauiPlainPureMode.Pages;

public partial class NATCheckPage : ContentPage
{
    ControlTemplate rfc3489Template = new ControlTemplate(typeof(RFC3489View));
    ControlTemplate rfc5780Template = new ControlTemplate(typeof(RFC5780View));

    public NATCheckPage()
    {
        InitializeComponent();

        contentView.ControlTemplate = rfc3489Template;
    }

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var radioBtn = sender as RadioButton;
        if (radioBtn != null && e.Value)
        {
            var viewModel = this.BindingContext as NATCheckViewModel;
            var value = radioBtn.Value.ToString();
            if (value == "NoProxy")
            {
                //viewModel.IsSocks5Selected = false;
                contentView.ControlTemplate = rfc3489Template;
            }
            else if (value == "Socks5")
            {
                //viewModel.IsSocks5Selected = true;
                contentView.ControlTemplate = rfc5780Template;
            }
        }
    }
}