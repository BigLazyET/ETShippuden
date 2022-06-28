using System;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoWireAttribute : Attribute
    {
    }
}
