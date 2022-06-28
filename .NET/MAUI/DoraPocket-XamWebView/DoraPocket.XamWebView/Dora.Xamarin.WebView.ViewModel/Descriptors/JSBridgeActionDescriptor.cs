using Dora.Xamarin.WebView.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// JSBridge方法描述信息
    /// </summary>
    public class JSBridgeActionDescriptor
    {
        private readonly Lazy<Func<BaseViewModel, string, ValueTask<string>>> jsBridgeNativeHandlerAccessor;

        /// <summary>
        /// ViewModel类型
        /// </summary>
        public Type ViewModelType { get; }

        /// <summary>
        /// JSBridge方法名称
        /// </summary>
        public string BridgeName { get; }

        /// <summary>
        /// JSBridge方法的描述
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// JSBridge方法参数的描述
        /// </summary>
        public ParameterInfo Parameter { get; }

        /// <summary>
        /// JSBridge方法的返回类型
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// JsBridge Action的委托实现
        /// </summary>
        public Func<BaseViewModel, string, ValueTask<string>> JSBridgeNativeHandler => jsBridgeNativeHandlerAccessor.Value;

        public JSBridgeActionDescriptor(Type viewModelType, string bridgeName, MethodInfo methodInfo)
        {
            ViewModelType = Guard.ArgumentNotNull(viewModelType, nameof(viewModelType));
            BridgeName = Guard.ArgumentNotWhiteSpace(bridgeName, nameof(bridgeName));
            MethodInfo = Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            var parameters = methodInfo.GetParameters();
            if (parameters.Length > 1)
            {
                // 局限性：JSBridge Action方法参数只能小于等于1个！
                throw new ArgumentException($"JSBridge Action '{methodInfo.Name}' has multiple parameters and" +
                    $"the count of parameters of the valid JSBridge Action must be one or zero.", nameof(methodInfo));
            }
            Parameter = parameters.SingleOrDefault();
            ReturnType = methodInfo.ReturnType;
            jsBridgeNativeHandlerAccessor = new Lazy<Func<BaseViewModel, string, ValueTask<string>>>(() => JSBridgeNativeHandlerBuilder.BuildJSBridgeNativeHandler(this));
        }
    }
}
