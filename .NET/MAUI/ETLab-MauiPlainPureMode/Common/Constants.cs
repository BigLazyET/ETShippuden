using System.Net;

namespace ETLab_MauiPlainPureMode
{
    public class Constants
    {
        public static IEnumerable<string> StunServers => new List<string>
        {
            @"stun.syncthing.net",
            @"stun.qq.com",
            @"stun.miwifi.com",
            @"stun.bige0.com",
            @"stun.stunprotocol.org"
        };
    }
}
