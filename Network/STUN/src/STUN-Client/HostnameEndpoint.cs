using System.Diagnostics.CodeAnalysis;

namespace STUN
{
    public class HostNameEndPoint
    {
        /// <summary>
        /// IPv4地址或IPv6地址或主机名
        /// </summary>
        public string HostName { get; init; }

        /// <summary>
        /// 端口号
        /// </summary>
        public ushort Port { get; init; }

        public static bool TryParse(string hostName, [NotNullWhen(true)] out HostNameEndPoint? result, ushort defaultPort = 0)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(hostName))
                return false;

            var hostLen = hostName.Length;
            var colonPos = hostName.LastIndexOf(':');

            if (colonPos > 0)
            {
                if (hostName[colonPos - 1] is ']') // 192.168.1.1[0-9]:8080
                    hostLen = colonPos;
                else if (hostName.AsSpan(0, colonPos).LastIndexOf(':') is -1)   // 192.168.1.1:8080
                    hostLen = colonPos;
            }

            var name = hostName[..hostLen];
            var uriHostNameType = Uri.CheckHostName(name);
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
            if (hostLen != hostName.Length && !ushort.TryParse(hostName.AsSpan(hostLen + 1), out defaultPort))    // 引用
                return false;

            result = new HostNameEndPoint { HostName = name, Port = defaultPort };
            return true;
        }
    }
}
