using STUN.Client;
using STUN.Enums;

namespace STUN.Extensions
{
    public interface IStunClientFactory
    {
        Task<IStunClient> GetClient(ProxyType proxyType, string proxyServer, string stunServer, string proxyUser, string proxyPwd);
    }
}
