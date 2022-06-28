using Dora.Xamarin.WebView.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// ViewModel的描述信息
    /// </summary>
    public class ViewModelDescriptor
    {
        /// <summary>
        /// 将ViewModel中定义的 【注册在DI中的服务类】属性|字段，赋予DI中对应的服务实现类
        /// 这些属性和字段必须要用[AtuoWire]标签标记
        /// 为什么要这个，因为JSBridge Action方法中用到很多这些服务类！
        /// </summary>
        private readonly Lazy<Action<BaseViewModel>> serviceInitializerAccessor;

        /// <summary>
        /// ViewModel类型
        /// </summary>
        public Type ViewModelType { get; }

        /// <summary>
        /// JSBridge Action的描述集合
        /// key {string} - bridgeName
        /// value {JSBridgeActionDescriptor} - bridge方法的描述
        /// </summary>
        public IDictionary<string, JSBridgeActionDescriptor> Actions { get; }

        /// <summary>
        /// 依赖注入的服务字段或属性
        /// </summary>
        public InjectedMemberDescriptor[] InjectedMembers { get; }

        /// <summary>
        /// ViewModel中【依赖注入的服务】字段或属性的实例化（从DI中获取对应服务实现类）
        /// </summary>
        public Action<BaseViewModel> ServiceInitializerAccessor => serviceInitializerAccessor.Value;

        public ViewModelDescriptor(Type viewModelType)
        {
            Guard.ArgumentNotNull(viewModelType, nameof(viewModelType));
            Actions = new Dictionary<string, JSBridgeActionDescriptor>();

            var jsBridgeActions = from method in viewModelType.GetMethods()
                                  let attribute = method.GetCustomAttribute<JSBridgeActionAttribute>()
                                  where attribute != null
                                  select (MethodInfo: method, Attribute: attribute);
            foreach (var jsBridgeAction in jsBridgeActions)
            {
                var jsBridgeActionDescriptor = new JSBridgeActionDescriptor(viewModelType, jsBridgeAction.Attribute.BridgeName, jsBridgeAction.MethodInfo);
                Actions[jsBridgeActionDescriptor.BridgeName] = jsBridgeActionDescriptor;
            }

            var injectedFields = from field in viewModelType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                 let attribute = field.GetCustomAttribute<AutoWireAttribute>()
                                 where attribute != null
                                 select field;
            var injectedProperties = from property in viewModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     let attribute = property.GetCustomAttribute<AutoWireAttribute>()
                                     where attribute != null
                                     select property;
            var injectedMembers = new List<InjectedMemberDescriptor>();
            injectedMembers.AddRange(injectedFields.Select(field => new InjectedMemberDescriptor(field)));
            injectedMembers.AddRange(injectedProperties.Select(property => new InjectedMemberDescriptor(property)));
            InjectedMembers = injectedMembers.ToArray();
            serviceInitializerAccessor = new Lazy<Action<BaseViewModel>>(() => JSBridgeNativeHandlerBuilder.BuildInjectedMembersInitializer(this));
        }
    }
}
