using Dns.Net.Clients;
using Microsoft;
using STUN.Client;
using STUN.Enums;
using System.Net;
using System.Net.Sockets;

namespace STUN.Extensions
{
    public class StunClientFactory : IStunClientFactory
    {
        const ushort DefaultStunServerPort = 3478;

        private readonly DefaultAClient _defaultAClient;
        private readonly DefaultAAAAClient _defaultAAAAClient;

        public StunClientFactory(DefaultAClient defaultAClient, DefaultAAAAClient defaultAAAAClient)
        {
            _defaultAClient = defaultAClient;
            _defaultAAAAClient = defaultAAAAClient;
        }

        public async Task<IStunClient> CreateClientAsync(StunClientCreateOption createOption)
        {
            Requires.NotNullOrEmpty(createOption.StunServer, nameof(createOption.StunServer));
            Verify.Operation(HostNameEndPoint.TryParse(createOption.StunServer, out var stunHostnameEndPoint, DefaultStunServerPort), "Wrong stun server");

            var udpProxy = createOption.UdpProxy;
            var localEndPoint = udpProxy.LocalEndPoint;
            var stunIp = localEndPoint.AddressFamily switch
            {
                AddressFamily.InterNetworkV6 => await _defaultAAAAClient.QueryAsync(stunHostnameEndPoint.HostName!),
                _ => await _defaultAClient.QueryAsync(stunHostnameEndPoint.HostName!)
            };

            IStunClient stunClient = createOption.StunProtocol switch
            {
                StunProtocolType.RFC3489 => new StunClient3489(new IPEndPoint(stunIp, stunHostnameEndPoint.Port), udpProxy),
                StunProtocolType.RFC5389 => new StunClient5389(new IPEndPoint(stunIp, stunHostnameEndPoint.Port), udpProxy, TimeSpan.FromSeconds(3)),
                _ => throw Assumes.NotReachable()
            };

            return stunClient;
        }
    }
}
