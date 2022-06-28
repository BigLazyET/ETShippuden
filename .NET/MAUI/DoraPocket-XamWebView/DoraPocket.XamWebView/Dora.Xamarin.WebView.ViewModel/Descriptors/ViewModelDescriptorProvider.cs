using Dora.Xamarin.WebView.Common;
using Dora.Xamarin.WebView.Common.Attributes;
using System;
using System.Collections.Concurrent;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// 代表提供基于特定ViewModel的ViewModelDescriptor的提供者
    /// </summary>
    [RegisterAsService(typeof(IViewModelDescriptorProvider))]
    public class ViewModelDescriptorProvider: IViewModelDescriptorProvider
    {
        private readonly ConcurrentDictionary<Type, ViewModelDescriptor> cache = new ConcurrentDictionary<Type, ViewModelDescriptor>();

        public ViewModelDescriptor GetViewModelDescriptor(Type viewModelType)
        {
            Guard.ArgumentNotNull(viewModelType, nameof(viewModelType));
            if (!typeof(BaseViewModel).IsAssignableFrom(viewModelType))
            {
                throw new ArgumentException($"ViewModle '{viewModelType.FullName}' is not derived from BaseViewModel.");
            }
            return cache.GetOrAdd(viewModelType, _ => new ViewModelDescriptor(viewModelType));
        }

    }
}
