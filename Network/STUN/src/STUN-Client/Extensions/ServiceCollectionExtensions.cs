using Dns.Net.Clients;
using Microsoft.Extensions.DependencyInjection;
using STUN.Extensions;
using STUN.Proxy;

namespace STUN
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStunService(this IServiceCollection service)
        {
            service.AddSingleton<DefaultDnsClient>();
            service.AddSingleton<DefaultAClient>();
            service.AddSingleton<DefaultAAAAClient>();

            service.AddSingleton<IUdpProxyFactory, UdpProxyFactory>();
            service.AddSingleton<IStunClientFactory, StunClientFactory>();

            return service;
        }
    }
}
