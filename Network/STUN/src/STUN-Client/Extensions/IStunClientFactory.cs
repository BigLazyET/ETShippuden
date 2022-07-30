using STUN.Client;
using STUN.Enums;
using System.Net;

namespace STUN.Extensions
{
    public interface IStunClientFactory
    {
        Task<IStunClient> CreateClientAsync(StunClientCreateOption createOption);
    }
}
