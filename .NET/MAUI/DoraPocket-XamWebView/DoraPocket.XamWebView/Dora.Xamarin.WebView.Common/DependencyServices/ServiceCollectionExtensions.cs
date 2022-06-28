using Dora.Xamarin.WebView.Common.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Dora.Xamarin.WebView.Common.DependencyServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, string assemblyName)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            Guard.ArgumentNotWhiteSpace(assemblyName, nameof(assemblyName));

            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            if (assembly is null)
                return services;
            var results = from type in assembly.GetExportedTypes()
                          let attributes = type.GetCustomAttributes<RegisterAsServiceAttribute>()
                          where attributes.Any()
                          select new { ImplementationType = type, ServiceTypes = attributes.SelectMany(s => s.ServiceTypes) };
            foreach (var item in results)
            {
                foreach (var serviceType in item.ServiceTypes)
                {
                    services.AddSingleton(serviceType, item.ImplementationType);
                }
            }
            //var startupAttribute = assembly.GetCustomAttribute<StartupAttribute>();
            //if (startupAttribute != null)
            //{
            //    var startup = (IStartup)Activator.CreateInstance(startupAttribute.Startup);
            //    startup.ConfigureServices(services);
            //}
            return services;
        }
    }
}
