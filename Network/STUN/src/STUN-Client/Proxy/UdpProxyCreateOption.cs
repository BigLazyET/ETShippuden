using STUN.Enums;
using System.Net;

namespace STUN.Proxy
{
    public class UdpProxyCreateOption
    {
        public ProxyType proxyType { get; set; }

        public string ProxyServer { get; set; }

        public string ProxyUsername { get; set; }

        public string ProxyPassword { get; set; }

        public IPEndPoint LocalEndPoint { get; set; }
    }
}
