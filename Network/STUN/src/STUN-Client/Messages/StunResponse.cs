using System.Net;

namespace STUN.Messages
{
    public record StunResponse(StunMessage5389 stunMessage, IPEndPoint remote, IPAddress localAddress);
}
