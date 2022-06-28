using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Xamarin.WebView.Services
{
    /// <summary>
    /// 各平台FilePath
    /// 1. https://stackoverflow.com/questions/51445888/read-file-in-shared-library-xamarin-c-sharp
    /// 2. https://stackoverflow.com/questions/39644351/xamarin-forms-how-to-deal-against-ios-includes-of-the-pcl-part
    /// 3. [File Handling in Xamarin.Forms]https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/files?tabs=windows
    /// 4. [Images in Xamarin.Forms]https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/images?tabs=windows
    /// </summary>
    public interface IPlatformPathResolver
    {
        void PlatformSepcificPathsShownList();
    }
}
