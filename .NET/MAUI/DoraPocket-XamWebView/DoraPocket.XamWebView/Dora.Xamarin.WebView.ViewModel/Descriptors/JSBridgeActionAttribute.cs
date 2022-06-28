using Dora.Xamarin.WebView.Common;
using System;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// JSBridge Action 标签
    /// 用于标记方法，此方法用作JSBridge用（用于被js调用的native方法）
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JSBridgeActionAttribute : Attribute
    {
        public string BridgeName { get; }

        public JSBridgeActionAttribute(string bridgeName)
        {
            BridgeName = Guard.ArgumentNotWhiteSpace(bridgeName, nameof(bridgeName));
        }
    }
}
