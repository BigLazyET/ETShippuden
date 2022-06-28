using Dora.Xamarin.WebView.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Dora.Xamarin.WebView.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}