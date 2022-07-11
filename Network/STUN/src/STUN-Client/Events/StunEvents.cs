using STUN.Enums;
using STUN.Messages;
using STUN.Proxy;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace STUN.Events
{
    public class StunEvents : IStunEvents
    {
        private readonly IUdpProxy _udpProxy;

        public StunEvents(IUdpProxy udpProxy)
        {
            _udpProxy = udpProxy;
        }

        public virtual TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public async ValueTask<StunResponse> BindingRequestAsync(IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, CancellationToken cancellationToken)
        {
            var request = new StunMessage5389
            {
                Header = new StunMessageHeader { StunMessageType = Enums.StunMessageType.BindingRequest }
            };
            var stunResponse = await RequestAsync(request, remoteEndPoint, receiveEndPoint, cancellationToken);
            return stunResponse;
        }

        public async ValueTask<StunResponse> ChangeRequestAsync(IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, bool changeIp, bool changePort, CancellationToken cancellationToken)
        {
            var request = new StunMessage5389
            {
                Header = new StunMessageHeader { StunMessageType = StunMessageType.BindingRequest },
                Attributes = new[] { StunAttributeExtensions.BuildChangeRequest(changeIp, changePort) }
            };
            var stunResponse = await RequestAsync(request, remoteEndPoint, receiveEndPoint, cancellationToken);
            return stunResponse;
        }

        public virtual async ValueTask<StunResponse> RequestAsync(StunMessage5389 requestMessage, IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(0x10000);  // 16^4 = 2^16 bytes
                var buffer = memoryOwner.Memory;
                var writeLen = requestMessage.WriteTo(buffer.Span);

                var sendLen = await _udpProxy.SendToAsync(buffer[..writeLen], SocketFlags.None, remoteEndPoint, cancellationToken);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(ReceiveTimeout);
                var response = await _udpProxy.ReceiveMessageFromAsync(buffer, SocketFlags.None, receiveEndPoint, cts.Token);

                StunMessage5389 stunMessage5389 = new();
                if (stunMessage5389.TryParse(buffer.Span[..response.ReceivedBytes]) && stunMessage5389.IsSameTransaction(requestMessage))
                {
                    return new StunResponse(stunMessage5389, (IPEndPoint)response.RemoteEndPoint, response.PacketInformation.Address);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"STUN Requst Error: {ex.Message}");
            }
            return default;
        }
    }
}
