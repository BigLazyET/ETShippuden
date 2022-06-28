using Dora.Xamarin.WebView.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// ViewModel中定义的 【注册在DI中的服务类】属性|字段 的描述信息
    /// </summary>
    public sealed class InjectedMemberDescriptor
    {
        /// <summary>
        /// ViewModel中定义的 【注册在DI中的服务类】字段
        /// </summary>
        public FieldInfo Field { get; }

        /// <summary>
        /// ViewModel中定义的 【注册在DI中的服务类】属性
        /// </summary>
        public PropertyInfo Property { get; }

        public InjectedMemberDescriptor(FieldInfo fieldInfo)
        {
            Field = Guard.ArgumentNotNull(fieldInfo, nameof(fieldInfo));
        }

        public InjectedMemberDescriptor(PropertyInfo propertyInfo)
        {
            Property = Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));
            if (Property.SetMethod is null)
            {
                throw new ArgumentException($"Property '{Property.Name}' without Setter method can not be injected.", nameof(Property));
            }
        }
    }
}
