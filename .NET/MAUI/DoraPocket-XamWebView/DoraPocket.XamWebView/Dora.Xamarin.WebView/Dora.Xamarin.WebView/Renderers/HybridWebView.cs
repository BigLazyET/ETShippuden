using Dora.Xamarin.WebView.Common.Bridges;
using Dora.Xamarin.WebView.Common.DependencyServices;
using Dora.Xamarin.WebView.Common.Interfaces;
using Dora.Xamarin.WebView.ViewModel;
using Dora.Xamarin.WebView.ViewModel.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xamarin.Forms;
using Dora.Xamarin.WebView.Common.Extensions;
using System.Threading.Tasks;

namespace Dora.Xamarin.WebView.Renderers
{
    public class HybridWebView : View
    {
        #region properties and fields
        private readonly IJsonNetSerializer jsonNetSerializer;
        public IViewModelDescriptorProvider ViewModelDescriptorProvider { get; }
        #endregion

        #region bindable properties
        /// <summary>
        /// 此webview对应的js intero-invoke-actions 的viewModel
        /// </summary>
        public BaseViewModel ViewModel
        {
            get { return (BaseViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            propertyName: "ViewModel",
            returnType: typeof(BaseViewModel),
            declaringType: typeof(HybridWebView),
            defaultValue: default(BaseViewModel));

        /// <summary>
        /// html中引用资源(包括但不限于文件，图片资源)的基于的baseUrl
        /// 用于LoadHtmlString
        /// </summary>
        public string BaseUrl
        {
            get { return (string)GetValue(BaseUrlProperty); }
            set { SetValue(BaseUrlProperty, value); }
        }

        public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(
            propertyName: "BaseUrl",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        /// <summary>
        /// Html Sting文本
        /// </summary>
        public string HtmlString
        {
            get { return (string)GetValue(HtmlStringProperty); }
            set { SetValue(HtmlStringProperty, value); }
        }
        public static readonly BindableProperty HtmlStringProperty = BindableProperty.Create(
            propertyName: "HtmlString",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        /// <summary>
        /// Html文档绝对地址
        /// </summary>
        public string AbsoluteFileUrl
        {
            get { return (string)GetValue(AbsoluteFileUrlProperty); }
            set { SetValue(AbsoluteFileUrlProperty, value); }
        }

        public static readonly BindableProperty AbsoluteFileUrlProperty = BindableProperty.Create(
            propertyName: "AbsoluteFileUrl",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        /// <summary>
        /// Html文档绝对地址对应的read access url
        /// </summary>
        public string AbsoluteReadAccessUrl
        {
            get { return (string)GetValue(AbsoluteReadAccessUrlProperty); }
            set { SetValue(AbsoluteReadAccessUrlProperty, value); }
        }

        public static readonly BindableProperty AbsoluteReadAccessUrlProperty = BindableProperty.Create(
            propertyName: "AbsoluteReadAccessUrl",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));
        #endregion

        public HybridWebView()
        {
            jsonNetSerializer = ServiceProviderAccessor.Current.GetRequiredService<IJsonNetSerializer>();
            ViewModelDescriptorProvider = ServiceProviderAccessor.Current.GetRequiredService<IViewModelDescriptorProvider>();
        }

        /// <summary>
        /// 接收js调用传递的信息，并处理
        /// 找到对应的viewModel的action，如有回调，执行完再调用回调
        /// </summary>
        /// <param name="scriptMessage"></param>
        public void FrontMessageReceived(string scriptMessage)
        {
            FrontMessage frontMessage;
            try
            {
                frontMessage = jsonNetSerializer.Deserialize<FrontMessage>(scriptMessage);
                Console.WriteLine($"Message from js invoke: {frontMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Desrialize FrontMessage that from js invoke failed: {ex.Message}");
                return;
            }

            if (frontMessage is null)
            {
                Console.WriteLine("Message from js invoke is null");
                return;
            } 

            if (ViewModel is null)
            {
                Console.WriteLine("ViewModel of hybridWebView is null");
                return;
            }  

            var viewModelDescriptor = ViewModelDescriptorProvider.GetViewModelDescriptor(ViewModel.GetType());
            if (!viewModelDescriptor.Actions.TryGetValue(frontMessage.ActionName, out var jsBridgeActionDescriptor))
            {
                Console.WriteLine($"Specific JSBridge '{frontMessage.ActionName}' does not exist. frontMessage: {frontMessage}");
                CallbackJs(frontMessage, () => new { Code = 0, Message = $"Specific JSBridge '{frontMessage.ActionName}' does not exist"});
                return;
            }

            Task<string> task;
            try
            {
                var valueTask = jsBridgeActionDescriptor.JSBridgeNativeHandler.Invoke(ViewModel, frontMessage.Data?.ToString());
                if (valueTask.IsCompleted)
                {
                    CallbackJs(frontMessage, () => valueTask.Result);
                    return;
                }
                    
                task = valueTask.AsTask();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSBridge {jsBridgeActionDescriptor.BridgeName} Exception: {ex}, frontMessage: {frontMessage}");
                CallbackJs(frontMessage, () => new { Code = 0, Message = $"JSBridge {jsBridgeActionDescriptor.BridgeName} Exception: {ex}" });
                return;
            }

            Device.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var result = await task;
                    CallbackJs(frontMessage, () => result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"JSBridge {jsBridgeActionDescriptor.BridgeName} Exception: {ex}, frontMessage: {frontMessage}");
                    CallbackJs(frontMessage, () => new { Code = 0, Message = $"JSBridge {jsBridgeActionDescriptor.BridgeName} Exception: {ex}" });
                }
            });
        }

        /// <summary>
        /// 根据是否需要回调callback
        /// 若需要进行回调，则native调用js规定的统一回调方法(window.bridge.nativeCB)，并传入所需的参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="frontMessage"></param>
        /// <param name="func"></param>
        private void CallbackJs<T>(FrontMessage frontMessage, Func<T> func)
        {
            if (!frontMessage.HasCallback)
                return;

            if (func == null)
                return;

            var data = string.Empty;
            var result = func();
            if (result is string str)
                data = str;
            else
                data = jsonNetSerializer.Serialize(result);

            var callJsMessage = new CallJsMessage
            {
                Data = data,
                MethodName = frontMessage.MethodName,
                Timer = frontMessage.Timer
            };
            var parameter = jsonNetSerializer.Serialize(callJsMessage);
            ViewModel.CallJsCallback(parameter);
        }
    }
}
