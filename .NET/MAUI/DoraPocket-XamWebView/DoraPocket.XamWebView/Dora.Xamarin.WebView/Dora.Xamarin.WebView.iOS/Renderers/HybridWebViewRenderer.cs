using Dora.Xamarin.WebView.iOS.Renderers;
using Dora.Xamarin.WebView.Renderers;
using Foundation;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebKit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace Dora.Xamarin.WebView.iOS.Renderers
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WKWebView>, IWKScriptMessageHandler
    {
        private WKUserContentController userController;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            //if (e.OldElement is HybridWebView webView and not null)
            if (e.OldElement != null && e.OldElement is HybridWebView webView)
            {
                userController?.RemoveAllUserScripts();
                userController?.RemoveScriptMessageHandler("dora");
                webView.PropertyChanged -= OnElementPropertyChanged;
            }

            if (e.NewElement != null && Control is null && e.NewElement is HybridWebView hybridWebView)
            {
                // inject js scripts for [js invoke csharp native funs|actions]
                userController = new WKUserContentController();
                var script = new WKUserScript(new NSString(GenerateNativeFuncsScripts(hybridWebView)), WKUserScriptInjectionTime.AtDocumentStart, false);
                userController.AddUserScript(script);
                userController.AddScriptMessageHandler(this, "dora");

                // set customize url scheme
                var config = new WKWebViewConfiguration { UserContentController = userController };
                config.Preferences.SetValueForKey(FromObject(true), new NSString("allowFileAccessFromFileURLs"));
                config.SetValueForKey(FromObject(true), new NSString("allowUniversalAccessFromFileURLs"));
                // https://stackoverflow.com/questions/24208229/wkwebview-and-nsurlprotocol-not-working
                var wkUrlSchemeHandler = new HybridWKWebViewUrlSchemeHandler();
                config.SetUrlSchemeHandler(wkUrlSchemeHandler, "dorapocket");

                // create renderer-native controls
                var wkWebView = new WKWebView(Frame, config)
                {
                    WeakNavigationDelegate = this,
                    // prevent js alert from swallowing
                    UIDelegate = new HybridWebViewUIDelegate()
                };

                // disable bounce
                wkWebView.ScrollView.Bounces = false;
                wkWebView.ScrollView.AlwaysBounceVertical = false;

                SetNativeControl(wkWebView);
            }

            // regist delegate for [csharp native invoke js funcs]
            RegistCallJs();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Element is null)
            {
                if (sender is HybridWebView webView && webView != null)
                    webView.PropertyChanged -= OnElementPropertyChanged;
                return;
            }

            base.OnElementPropertyChanged(sender, e);

            var propertyName = e.PropertyName;
            if (propertyName == HybridWebView.BaseUrlProperty.PropertyName || propertyName == HybridWebView.HtmlStringProperty.PropertyName)
                LoadHtmlString(Element.HtmlString, Element.BaseUrl);
            else if (propertyName == HybridWebView.AbsoluteFileUrlProperty.PropertyName || propertyName == HybridWebView.AbsoluteReadAccessUrlProperty.PropertyName)
                LoadFileUrl(Element.AbsoluteFileUrl, Element.AbsoluteReadAccessUrl);

        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (message == null || message.Body == null)
                return;
            var scriptMessage = message.Body.ToString();
            if (string.IsNullOrWhiteSpace(scriptMessage))
                return;
            Console.WriteLine($"frontMessage: {scriptMessage}");
            Element.FrontMessageReceived(scriptMessage);
        }

        /// <summary>
        /// 构建注入到webview中的js，用于js调用
        /// 构建invokeCSharpAction函数，用于js调用，进而native通过DidReceiveScriptMessage获取message|data
        /// 构建window.bridge.nativeFuncs集合，用于存储native funcs|actions，从而js可以知道调用native的哪个方法Action
        /// </summary>
        /// <param name="hybridWebView"></param>
        /// <returns></returns>
        private string GenerateNativeFuncsScripts(HybridWebView hybridWebView)
        {
            var builder = new StringBuilder();
            builder.Append("function invokeCSharpAction(data) {window.webkit.messageHandlers.dora.postmessage(data);}");

            var viewModelDescriptor = hybridWebView.ViewModelDescriptorProvider.GetViewModelDescriptor(hybridWebView.ViewModel.GetType());
            var bridgeNames = viewModelDescriptor.Actions.Keys?.ToList();
            if (bridgeNames == null || bridgeNames.Count == 0)
                return null;
            builder.Append("window.bridge = window.bridge || {}; window.bridge.nativeFuncs = new Set([");
            for (int i = 0; i < bridgeNames.Count; i++)
            {
                if (i == bridgeNames.Count - 1)
                    builder.Append($"\"{bridgeNames[i]}\"");
                else
                    builder.Append($"\"{bridgeNames[i]}\",");
            }
            builder.Append($"]);");
            builder.Append("window.NativeFuncsReady && window.NativeFuncsReady();");    // ？

            Console.WriteLine($"inject js: {builder}");

            return builder.ToString();
        }

        /// <summary>
        /// 加载html sring字符串
        /// </summary>
        /// <param name="htmlString">html string字符串</param>
        /// <param name="baseUrl">html中引用资源(包括但不限于文件，图片资源)的基于的baseUrl</param>
        private void LoadHtmlString(string htmlString, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(htmlString))
                return;

            Control.LoadHtmlString(htmlString, new NSUrl(baseUrl, true));
        }

        /// <summary>
        /// 加载html文件
        /// </summary>
        /// <param name="absoluteFileUrl"></param>
        /// <param name="absoluteReadAccessUrl"></param>
        private void LoadFileUrl(string absoluteFileUrl, string absoluteReadAccessUrl)
        {
            if (string.IsNullOrWhiteSpace(absoluteFileUrl))
                return;
            if (string.IsNullOrWhiteSpace(absoluteReadAccessUrl))
                return;

            Control.LoadFileUrl(new NSUrl($"file://{absoluteFileUrl}"), new NSUrl($"file://{absoluteReadAccessUrl}"));
        }

        /// <summary>
        /// csharp调用js方法
        /// </summary>
        private void RegistCallJs()
        {
            if (Element is null || Element.ViewModel is null)
                return;

            Element.ViewModel.SetCallJsHandlerBuilder(async evaluateStr =>
            {
                Task<NSObject> task;
                NSObject result = null;
                try
                {
                    if (MainThread.IsMainThread)
                        task = Control.EvaluateJavaScriptAsync(evaluateStr);
                    else
                        task = MainThread.InvokeOnMainThreadAsync(() => Control.EvaluateJavaScriptAsync(evaluateStr));
                    result = await task;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"csharp invoke js function {evaluateStr} failed: {ex.Message}");
                }
                return result?.ToString();

            });
        }
    }
}