using System.Diagnostics.CodeAnalysis;

namespace STUN
{
    public class StunServer
    {
        const ushort DefaultPort = 3478;

        public string Hostname { get; init; } = @"stun.syncthing.net";

        public ushort Port { get; init; } = DefaultPort;

        public static bool TryParse(string s, [NotNullWhen(true)] out StunServer? result)
        {
            result = null;
            if (!HostnameEndpoint.TryParse(s, out HostnameEndpoint? host, DefaultPort))
                return false;

            result = new StunServer { Hostname = host.Hostname, Port = host.Port };
            return true;
        }
    }
}
