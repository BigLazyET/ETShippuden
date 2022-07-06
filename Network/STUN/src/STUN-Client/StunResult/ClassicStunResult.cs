using STUN.Enums;

namespace STUN
{
    public class ClassicStunResult : StunResult
    {
        public NATType NATType { get; set; } = NATType.Unknown;

        public void Clone(ClassicStunResult result)
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
