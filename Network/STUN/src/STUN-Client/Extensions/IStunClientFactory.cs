using STUN.Client;
using STUN.Enums;

namespace STUN.Extensions
{
    public interface IStunClientFactory
    {
        IStunClient GetClient(ProxyType proxyType);
    }
}
