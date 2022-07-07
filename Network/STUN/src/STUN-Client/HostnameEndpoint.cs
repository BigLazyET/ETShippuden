using System.Diagnostics.CodeAnalysis;

namespace STUN
{
    public class HostNameEndPoint
    {
        public string Hostname { get; init; }

        public ushort Port { get; init; }

        public static bool TryParse(string s, [NotNullWhen(true)] out HostNameEndPoint? result, ushort defaultPort = 0)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(s))
                return false;

            var hostLen = s.Length;
            var colonPos = s.LastIndexOf(':');

            if (colonPos > 0)
            {
                if (s[colonPos - 1] is ']') // 192.168.1.1[0-9]:8080
                    hostLen = colonPos;
                else if (s.AsSpan(0, colonPos).LastIndexOf(':') is -1)   // 192.168.1.1:8080
                    hostLen = colonPos;
            }

            var host = s[..hostLen];
            var uriHostNameType = Uri.CheckHostName(host);
            var isValidType = uriHostNameType switch
            {
                UriHostNameType.Dns => true,
                UriHostNameType.IPv4 => true,
                UriHostNameType.IPv6 => true,
                _ => false
            };
            if (!isValidType)
                return isValidType;

            // if (hostLen != s.Length && !ushort.TryParse(s[..(hostLen + 1)], out defaultPort))    // 新开辟空间
            if (hostLen != s.Length && !ushort.TryParse(s.AsSpan(hostLen + 1), out defaultPort))    // 引用
                return false;
            
            result = new HostNameEndPoint { Hostname = host, Port = defaultPort };
            return true;
        }
    }
}
