using Dora.Xamarin.WebView.Common.DependencyServices;
using Dora.Xamarin.WebView.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Compression;

namespace Dora.Xamarin.WebView.Common
{
    /// <summary>
    /// [File Handling in Xamarin.Forms]https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/files
    /// </summary>
    public class HybridViewPathResolver : IHybridViewPathResolver
    {
        private readonly IResourcePathResolver resourcePathResolver;

        public string CurrentVersion { get; }

        public HybridViewPathResolver()
        {
            resourcePathResolver = ServiceProviderAccessor.Current.GetRequiredService<IResourcePathResolver>();
        }

        public string GetIndexHtmlPath()
        {
            var currentVersionHybridPath = resourcePathResolver.GetExtractedHybridVersionPath();
            return Path.Combine(currentVersionHybridPath, "index.html");
        }

        public string GetIndexJsonPath()
        {
            var currentVersionHybridPath = resourcePathResolver.GetExtractedHybridVersionPath();
            return Path.Combine(currentVersionHybridPath, "index.json");
        }

        public void ExtractZip()
        {
            
        }
    }
}
