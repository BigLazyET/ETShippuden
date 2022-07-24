using STUN.Enums;
using System.Net;

namespace STUN
{
    public class StunResult
    {
        public NATType NATType { get; set; } = NATType.Unknown;

        public IPEndPoint? PublicEndPoint { get; set; }

        public IPEndPoint? LocalEndPoint { get; set; }

        public virtual void Reset()
        {
            PublicEndPoint = default;
            LocalEndPoint = default;
            NATType = NATType.Unknown;
        }
    }
}
