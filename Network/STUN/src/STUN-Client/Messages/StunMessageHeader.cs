using STUN.Enums;
using System.Security.Cryptography;

namespace STUN.Messages
{
    /// <summary>
    /// RFC 5389 协议对Stun message的规定：https://tools.ietf.org/html/rfc5389#section-6
    /// /*
    ///    Format of STUN Message Header - 20-bytes length
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
        public StunMessageType StunMessageType { get; set; } = StunMessageType.BindingRequest;

        /// <summary>
        /// RFC3489和RFC5389的不同之处
        /// 前者的magic cookie是transaction id的一部分
        /// 后者的magic cookie则必须包含0x2112A442这部分
        /// </summary>
        public uint MagicCookie { get; set; } = 0x2112A442;

        public byte[] TransactionId { get; set; } = new byte[12];    // 随机数填充即可

        public int MessageLength { get; set; }

        public StunMessageHeader()
        {
            RandomNumberGenerator.Fill(TransactionId);
        }
    }
}
