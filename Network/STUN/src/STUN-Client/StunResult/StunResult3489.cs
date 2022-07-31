using STUN.Enums;

namespace STUN
{
    public class StunResult3489 : StunResult
    {
        public void Clone(StunResult3489 result)
        {
            PublicEndPoint = result.PublicEndPoint;
            ActualLocalEndPoint = result.ActualLocalEndPoint;
            NATType = result.NATType;
        }
    }
}
