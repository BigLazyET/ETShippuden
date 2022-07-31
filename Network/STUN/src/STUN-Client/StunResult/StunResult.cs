using STUN.Enums;
using System.Net;

namespace STUN
{
    public class StunResult
    {
        public NatType NATType { get; set; } = NatType.Unknown;

        public IPEndPoint? PublicEndPoint { get; set; }

        public IPEndPoint? ActualLocalEndPoint { get; set; }

        public virtual void Reset()
        {
            PublicEndPoint = default;
            ActualLocalEndPoint = default;
            NATType = NatType.Unknown;
        }
    }
}
