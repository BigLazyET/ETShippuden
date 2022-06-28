using System;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// 代表提供基于特定ViewModel的ViewModelDescriptor的提供者
    /// </summary>
    public interface IViewModelDescriptorProvider
    {
        /// <summary>
        /// 根据ViewModel类型获取ViewMoel的描述信息
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        ViewModelDescriptor GetViewModelDescriptor(Type viewModelType);
    }
}
