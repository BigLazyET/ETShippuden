using System.Diagnostics.CodeAnalysis;

namespace STUN
{
    public class StunServer
    {
        const ushort DefaultPort = 3478;

        /// <summary>
        /// IPv4地址或IPv6地址或主机名
        /// </summary>
        public string Hostname { get; init; } = @"stun.syncthing.net";

        public ushort Port { get; init; } = DefaultPort;

        public static bool TryParse(string hostName, [NotNullWhen(true)] out StunServer? result)
        {
            result = null;

            if (!HostNameEndPoint.TryParse(hostName, out HostNameEndPoint? hostNameEndPoint, DefaultPort))
                return false;

            result = new StunServer { Hostname = hostNameEndPoint.Hostname, Port = hostNameEndPoint.Port };
            return true;
        }
    }
}
