using STUN.Enums;
using STUN.Proxy;

namespace STUN.Client
{
    public class StunClientCreateOption
    {
        public IUdpProxy UdpProxy { get; set; }

        public StunProtocolType StunProtocol { get; set; }

        public string StunServer { get; set; }
    }
}
