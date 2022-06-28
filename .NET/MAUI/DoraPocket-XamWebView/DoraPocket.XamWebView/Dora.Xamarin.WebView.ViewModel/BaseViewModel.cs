using Dora.Xamarin.WebView.Common;
using Dora.Xamarin.WebView.Common.DependencyServices;
using Dora.Xamarin.WebView.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dora.Xamarin.WebView.Common.Extensions;
using Dora.Xamarin.WebView.ViewModel.Descriptors;

namespace Dora.Xamarin.WebView.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private readonly IJsonNetSerializer jsonNetSerializer;
        // Func in string: js方法+参数，形如"function(params)"，也就是GenerateJsFunction方法提供的
        // Func out Task<string> -> string: js返回的对象序列化字符串，native拿到需要反序列化成所需要的对象TOutput
        private Func<string, Task<string>> callJsHandler;

        protected IServiceProvider ServiceProvider { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChange = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChange?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public BaseViewModel()
        {
            jsonNetSerializer = ServiceProviderAccessor.Current.GetRequiredService<IJsonNetSerializer>();

            ServiceProvider = ServiceProviderAccessor.Current;
            var viewModelDescriptorProvider = ServiceProvider.GetRequiredService<IViewModelDescriptorProvider>();
            var viewModelDescriptor = viewModelDescriptorProvider.GetViewModelDescriptor(GetType());
            if (viewModelDescriptor.InjectedMembers.Length > 0)
            {
                // 为了jsbridge action的顺利执行，提前将action中使用的服务初始化好，该服务用[AutoWire]标识
                // 当然如果没有用此标识的服务类，可以在action执行前初始化好就行，比如在类的构造中就需要的服务初始化即可
                viewModelDescriptor.ServiceInitializerAccessor(this);
            }
        }

        #region JS Bridge
        public void SetCallJsHandlerBuilder(Func<string, Task<string>> builder)  
        {
            callJsHandler = Guard.ArgumentNotNull(builder, nameof(builder));
        }

        // js 统一收口(native主动调用js回调)方法为 window.bridge.callJs
        public Task CallJsCallback<TInput>(TInput argument) => CallJs<TInput, object>("window.bridge.nativeCB", argument);

        public Task<TOutput> CallJsCallback<TInput, TOutput>(TInput argument) => CallJs<TInput,TOutput>("window.bridge.nativeCB", argument);

        // js 统一收口(native主动调用js)方法为 window.bridge.callJs
        public Task CallJs<TInput>(TInput argument) => CallJs<TInput, object>("window.bridge.callJs", argument);

        public Task<TOutput> CallJs<TInput, TOutput>(TInput argument) => CallJs<TInput, TOutput>("window.bridge.callJs", argument);

        /// <summary>
        /// native invoke js
        /// </summary>
        /// <typeparam name="TInput">js参数</typeparam>
        /// <typeparam name="TOutput">返回结果</typeparam>
        /// <param name="jsFunctionName">js方法名</param>
        /// <param name="argument">string-对象序列化的结果;type-对象</param>
        /// <returns></returns>
        public async Task<TOutput> CallJs<TInput, TOutput>(string jsFunctionName, TInput argument)
        {
            Guard.ArgumentNotNull(jsFunctionName, nameof(jsFunctionName));
            if (callJsHandler is null)
                throw new InvalidOperationException("Call Js handler is not set by SetCallJsHandlerBuilder");

            string parameters;
            if (argument is string param)
                parameters = param;
            else
                parameters = jsonNetSerializer.Serialize(argument);

            var script = GenerateJsFunction(jsFunctionName, parameters);

            var result = await callJsHandler(script);

            return jsonNetSerializer.Deserialize<TOutput>(result);
        }

        /// <summary>
        /// native invoke js 的{方法|参数}
        /// </summary>
        /// <param name="jsFunctionName">js方法名</param>
        /// <param name="argument">对象序列化字符串</param>
        /// <returns></returns>
        private string GenerateJsFunction(string jsFunctionName, string argument)
        {
            Console.WriteLine($"call js argument：{argument}");
            // "{\"XPath\":5.0,\"YPath\":6.0}" -> 文本显示：{"XPath":5.0,"YPath":6.0}
            var serializedArgument = jsonNetSerializer.Serialize(argument);
            
            // "\"{\\\"XPath\\\":5.0,\\\"YPath\\\":6.0}\"" -> 文本显示："{\"XPath\":5.0,\"YPath\":6.0}"
            Console.WriteLine($"call js paraStr more serialized：{serializedArgument}");

            var script = $"{jsFunctionName}({argument})";
            Console.WriteLine($"call js script: {script}");
            return script;
        }
        #endregion
    }
}
