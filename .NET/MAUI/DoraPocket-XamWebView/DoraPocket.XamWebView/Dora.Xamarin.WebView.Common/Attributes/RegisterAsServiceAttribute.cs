using System;
using System.Linq;

namespace Dora.Xamarin.WebView.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterAsServiceAttribute : Attribute
    {
        public Type[] ServiceTypes { get; }

        public RegisterAsServiceAttribute(Type serviceType)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            ServiceTypes = new[] { serviceType };
        }

        public RegisterAsServiceAttribute(Type serviceType, params Type[] serviceTypes)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            if (serviceTypes is null)
                ServiceTypes = new[] { serviceType };
            else
                ServiceTypes = serviceTypes.Concat(new[] { serviceType }).ToArray();
        }
    }
}
