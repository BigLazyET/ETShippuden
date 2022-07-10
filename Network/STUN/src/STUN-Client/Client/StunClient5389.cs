using Microsoft;
using STUN.Enums;
using STUN.Messages;
using STUN.Proxy;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Client
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5389#section-7.2.1
    /// https://tools.ietf.org/html/rfc5780#section-4.2
    /// Mapping behavior和Filtering behavior：https://cloud.tencent.com/developer/article/1760086
    /// Mapping指的是内网IP映射成外网IP的这个Map
    /// Filtering指的是NAT判断并过滤外网发过来的包能否通过(Mapping映射过的外网IP)发给内网IP
    /// </summary>
    public class StunClient5389 : IStunClient
    {
        public virtual IPEndPoint LocalEndPoint => (IPEndPoint)_udpProxy.Client.LocalEndPoint!;

        public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(3);

        private readonly IPEndPoint _remoteEndPoint;

        private readonly IUdpProxy _udpProxy;

        private StunResult5389 StunResult5389 = new();

        public StunClient5389(IPEndPoint remoteEndPoint, IPEndPoint localEndPoint, IUdpProxy udpProxy)
        {
            Requires.NotNull(remoteEndPoint, nameof(remoteEndPoint));
            Requires.NotNull(localEndPoint, nameof(localEndPoint));

            _remoteEndPoint = remoteEndPoint;
            _udpProxy = udpProxy ?? new NoneUdpProxy(localEndPoint);
            StunResult5389.LocalEndPoint = localEndPoint;   // 无意义
        }

        public async ValueTask CloseProxyAsync(CancellationToken cancellationToken = default)
        {
            await _udpProxy.CloseAsync(cancellationToken);
        }

        public async ValueTask ConnectProxyAsync(CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ReceiveTimeout);

            await _udpProxy.ConnectAsync(cts.Token);
        }

        public void Dispose()
        {
            _udpProxy.Dispose();
        }

        public ValueTask QueryAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        private async ValueTask FilteringBehaviorAsync(CancellationToken cancellationToken)
        {
            var bindingResponse = await BindingRequestAsync(_remoteEndPoint, _remoteEndPoint, cancellationToken);
            var stunServerMappedIPEndPoint = bindingResponse.stunMessage.GetIPEndPointFromXorMappedAddressAttribute();
            var stunServerAnotherIPEndPoint = bindingResponse.stunMessage.GetIPEndPointFromChangedAddressAttribute();

            StunResult5389.LocalEndPoint = bindingResponse is null ? null : new IPEndPoint(bindingResponse.localAddress, LocalEndPoint.Port);
            StunResult5389.PublicEndPoint = stunServerMappedIPEndPoint;
            StunResult5389.OtherEndPoint = stunServerAnotherIPEndPoint;
            StunResult5389.BindingTestResult = BindingTestResult.Success;

            if (bindingResponse is null)
            {
                StunResult5389.BindingTestResult = BindingTestResult.Fail;
                return;
            }  
            else if (stunServerMappedIPEndPoint is null)
            {
                StunResult5389.BindingTestResult = BindingTestResult.UnSupportedServer;
                return;
            }
            if(!HasValidOtherAddress(stunServerAnotherIPEndPoint))
            {
                StunResult5389.FilteringBehavior = FilteringBehavior.UnSupportedServer;
                return;
            }

            
        }

        private async ValueTask<StunResponse> BindingRequestAsync(IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, CancellationToken cancellationToken)
        {
            var request = new StunMessage5389
            {
                Header = new StunMessageHeader { StunMessageType = Enums.StunMessageType.BindingRequest }
            };
            var stunResponse = await RequestAsync(request, remoteEndPoint, receiveEndPoint, cancellationToken);
            return stunResponse;
        }

        private async ValueTask<StunResponse> ChangeRequestAsync(IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, bool changeIp, bool changePort, CancellationToken cancellationToken)
        {
            var request = new StunMessage5389
            {
                Header = new StunMessageHeader { StunMessageType = StunMessageType.BindingRequest },
                Attributes = new[] { StunAttributeExtensions.BuildChangeRequest(changeIp, changePort) }
            };
            var stunResponse = await RequestAsync(request, remoteEndPoint, receiveEndPoint, cancellationToken);
            return stunResponse;
        }

        private async ValueTask<StunResponse> RequestAsync(StunMessage5389 requestMessage, IPEndPoint remoteEndPoint, IPEndPoint receiveEndPoint, CancellationToken cancellationToken)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasValidOtherAddress([NotNullWhen(true)] IPEndPoint? other)
        {
            return other is not null
                   && !Equals(other.Address, _remoteEndPoint.Address)
                   && other.Port != _remoteEndPoint.Port;
        }
    }
}
