using Foundation;
using System;
using System.IO;
using System.Net;
using WebKit;

namespace Dora.Xamarin.WebView.iOS.Renderers
{
    /// <summary>
    /// 用于h5页面中的url资源请求，只限于本地资源
    /// 包括但不限于文件，图片等等
    /// 注意，这些请求url请以规定的scheme开头： dorapocket://storage/
    /// </summary>
    public class HybridWKWebViewUrlSchemeHandler : NSObject, IWKUrlSchemeHandler
    {
        public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
        {
            var requestUrl = urlSchemeTask.Request.Url.AbsoluteString;
            if (string.IsNullOrWhiteSpace(requestUrl))
                return;
            Console.WriteLine($"urlSchemeTask.Request.Url: {requestUrl}");

            var decodeUrl = WebUtility.UrlDecode(requestUrl);
            Console.WriteLine($"decodeUrl: {decodeUrl}");

            var isUrlFormat = requestUrl.StartsWith("dorapocket://storage/", StringComparison.OrdinalIgnoreCase);
            if (!isUrlFormat)
                return;

            var filePath = decodeUrl.Replace("dorapocket://storage",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Storage"), StringComparison.OrdinalIgnoreCase); // ？
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                var response = new NSUrlResponse(urlSchemeTask.Request.Url, "application/*", (nint)fileInfo.Length, null);
                urlSchemeTask.DidReceiveResponse(response);
                urlSchemeTask.DidReceiveData(NSData.FromFile(filePath));
            }
            else
            {
                var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, 404, "HTTP/1.1", new NSDictionary());
                urlSchemeTask.DidReceiveResponse(response);
            }
            urlSchemeTask.DidFinish();
        }

        public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask) { }
    }
}