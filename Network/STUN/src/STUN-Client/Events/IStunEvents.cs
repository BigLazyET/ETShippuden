using STUN.Messages;
using System.Net;

namespace STUN.Events
{
    public interface IStunEvents
    {
        TimeSpan ReceiveTimeout { get; set; }

        ValueTask<StunResponse> BindingRequestAsync(IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, CancellationToken cancellationToken);

        ValueTask<StunResponse> ChangeRequestAsync(IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, bool changeIp, bool changePort, CancellationToken cancellationToken);

        ValueTask<StunResponse> RequestAsync(StunMessage5389 requestMessage, IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, CancellationToken cancellationToken);
    }
}
