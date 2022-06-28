using System;
using UIKit;
using WebKit;

namespace Dora.Xamarin.WebView.iOS.Renderers
{
    /// <summary>
    /// 防止javascript alert被吞
    /// </summary>
    public class HybridWebViewUIDelegate : WKUIDelegate
    {
        public override void RunJavaScriptAlertPanel(WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
        {
            // base.RunJavaScriptAlertPanel(webView, message, frame, completionHandler);

            var alertController = UIAlertController.Create("警告", message, UIAlertControllerStyle.Alert);
            var cancelAction = UIAlertAction.Create("取消", UIAlertActionStyle.Cancel, _ => completionHandler());
            alertController.AddAction(cancelAction);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        public override void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
        {
            // base.RunJavaScriptConfirmPanel(webView, message, frame, completionHandler);

            var alertController = UIAlertController.Create("提醒", message, UIAlertControllerStyle.Alert);
            var confirmAction = UIAlertAction.Create("确认", UIAlertActionStyle.Default, _ => completionHandler(true));
            var cancelAction = UIAlertAction.Create("取消", UIAlertActionStyle.Cancel, _ => completionHandler(false));
            alertController.AddAction(confirmAction);
            alertController.AddAction(cancelAction);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        public override void RunJavaScriptTextInputPanel(WKWebView webView, string prompt, string defaultText, WKFrameInfo frame, Action<string> completionHandler)
        {
            // base.RunJavaScriptTextInputPanel(webView, prompt, defaultText, frame, completionHandler);

            var alertController = UIAlertController.Create(prompt, null, UIAlertControllerStyle.Alert);
            alertController.AddTextField(tf => tf.Placeholder = defaultText);
            var confirmAction = UIAlertAction.Create("确认", UIAlertActionStyle.Default, _ => completionHandler(alertController.TextFields[0].Text));
            alertController.AddAction(confirmAction);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }
    }
}