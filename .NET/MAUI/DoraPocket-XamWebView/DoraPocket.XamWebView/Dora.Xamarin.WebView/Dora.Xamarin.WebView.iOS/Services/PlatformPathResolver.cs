using Dora.Xamarin.WebView.iOS.Services;
using Dora.Xamarin.WebView.Services;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformPathResolver))]
namespace Dora.Xamarin.WebView.iOS.Services
{
    public class PlatformPathResolver : IPlatformPathResolver
    {
        public void PlatformSepcificPathsShownList()
        {
            Console.WriteLine($"---------------------iOS of NSBundle.MainBundle---------------------");

            Console.WriteLine($"BuiltinPluginsPath: {NSBundle.MainBundle.BuiltinPluginsPath}");
            Console.WriteLine($"BuiltinPluginsPath: {NSBundle.MainBundle.BuiltinPluginsPath}");
            Console.WriteLine($"BuiltinPluginsPath: {NSBundle.MainBundle.BuiltinPluginsPath}");
            Console.WriteLine($"BuiltinPluginsPath: {NSBundle.MainBundle.BuiltinPluginsPath}");
            Console.WriteLine($"BuiltinPluginsPath: {NSBundle.MainBundle.BuiltinPluginsPath}");
        }
    }
}