using STUN.Enums;

namespace STUN
{
    public class StunResult3489 : StunResult
    {
        public NATType NATType { get; set; } = NATType.Unknown;

        public void Clone(StunResult3489 result)
        {
            PublicEndPoint = result.PublicEndPoint;
            LocalEndPoint = result.LocalEndPoint;
            NATType = result.NATType;
        }

        public override void Reset()
        {
            base.Reset();
            NATType = NATType.Unknown;
        }
    }
}
