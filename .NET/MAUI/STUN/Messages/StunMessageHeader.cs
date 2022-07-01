using STUN.Enums;

namespace STUN.Messages
{
    /// <summary>
    /// RFC 5389 协议对Stun message的规定：https://tools.ietf.org/html/rfc5389#section-6
    /// /*
    ///    Format of STUN Message Header
    ///    
    ///     0                   1                   2                   3
    ///     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    |         Type                  |            Length             |
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    |                         Magic Cookie                          |
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    |                                                               |
    ///    |                     Transaction ID(96 bits)                   |
    ///    |                                                               |
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// */
    /// </summary>
    public class StunMessageHeader
    {
        public StunMessageType StunMessageType { get; set; }

        public uint MagicCookie { get; set; }

        public byte[] TransactionId { get; }

        public int MessageLength { get; set; }
    }
}
