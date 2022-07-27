using Microsoft.Extensions.DependencyInjection;
using STUN.Extensions;
using STUN.Proxy;

namespace STUN
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStunService(this IServiceCollection service)
        {
            service.AddSingleton<IProxyFactory, ProxyFactory>();
            service.AddSingleton<IStunClientFactory, StunClientFactory>();

            return service;
        }
    }
}
