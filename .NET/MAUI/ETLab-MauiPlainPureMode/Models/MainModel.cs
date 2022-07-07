using ETLab_MauiPlainPureMode.ViewModels;

namespace ETLab_MauiPlainPureMode.Models
{
    public class MainModel
    {
    }

    public class PageNavigation : ObservableObject
    {
        private string _pageName;
        public string PageName { get { return _pageName; } set { SetProperty(ref _pageName, value); } }

        private string _pageUrl;
        public string PageUrl
        {
            get { return _pageUrl; }
            set { SetProperty(ref _pageUrl, value); }
        }
    }
}
