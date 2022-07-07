using ETLab_MauiPlainPureMode.Models;
using Microsoft;
using System.Windows.Input;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public List<PageNavigation> PageNavigations { get; }

        public ICommand PageSelectedCommand => new Command<object>(PageSelectedAsync);

        public MainViewModel()
        {
            PageNavigations = new()
            {
                new(){ PageName = "RFC3489", PageUrl="//nat/stun/rfc3489" },
                new(){ PageName = "RFC5389", PageUrl="//nat/stun/rfc5389" },
                new(){ PageName = "关于", PageUrl = "//about" }
            };
        }

        private async void PageSelectedAsync(object pageNavigation)
        {
            Requires.NotNull(pageNavigation, nameof(pageNavigation));

            if (pageNavigation is PageNavigation navigation && navigation is not null)
                await Shell.Current.GoToAsync(navigation.PageUrl);
            else
                await Application.Current.MainPage.DisplayAlert("提醒", "未解析到页面地址", "了解");
        }
    }
}
