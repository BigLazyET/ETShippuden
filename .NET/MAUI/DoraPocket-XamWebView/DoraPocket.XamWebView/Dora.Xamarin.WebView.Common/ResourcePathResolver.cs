using Dora.Xamarin.WebView.Common.Attributes;
using Dora.Xamarin.WebView.Common.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dora.Xamarin.WebView.Common
{
    /// <summary>
    /// 文件资源路径处理器
    /// 目前先确定，后期可以通过配置json来动态设置相应的路径！
    /// </summary>
    [RegisterAsService(typeof(IResourcePathResolver))]
    public class ResourcePathResolver : IResourcePathResolver
    {
        public string DocumentsDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string GetExtractedHybridBasePath()
        {
            return Path.Combine(DocumentsDirectory, "Hybrid");
        }

        public string GetExtractedHybridVersionPath()
        {
            // hybrid zip命名必须形如{数字版本}.zip；后期可通过Json来动态指定
            var regex = new Regex(@"^[0-9]*$");
            var extractedHybridBasePath = new DirectoryInfo(GetExtractedHybridBasePath());
            var hybridVersionFolder = extractedHybridBasePath.EnumerateDirectories()
                .Where(x => regex.IsMatch(x.Name))
                .OrderByDescending(x => int.Parse(x.Name))
                .FirstOrDefault();

            Console.WriteLine($"current version hybrid folder: {hybridVersionFolder.FullName}");
            return hybridVersionFolder.FullName;
        }
    }
}
