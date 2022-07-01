using System.Net;

namespace STUN
{
    public class StunResult
    {
        public IPEndPoint? PublicEndPoint { get; set; }

        public IPEndPoint? LocalEndPoint { get; set; }

        public virtual void Reset()
        {
            PublicEndPoint = default;
            LocalEndPoint = default;
        }
    }
}
