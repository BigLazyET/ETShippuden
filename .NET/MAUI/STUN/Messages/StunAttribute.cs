using Microsoft;
using STUN.Enums;
using System.Buffers.Binary;

namespace STUN.Messages
{
    /// <summary>
    /// RFC 5389 协议对Stun message的规定：https://tools.ietf.org/html/rfc5389#section-6
    /// /*
    ///    Format of STUN Attributes
    /// 
    ///    Length 是大端
    ///    必须4字节对齐
    ///    对齐的字节可以是任意值
    ///     0                   1                   2                   3
    ///     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    |         Type                  |            Length             |
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    |                         Value(variable)                ....
    ///    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// */
    /// */
    /// </summary>
    public class StunAttribute
    {
        public StunAttributeType StunAttributeType { get; set; } = StunAttributeType.Useless;

        public ushort Length { get; set; }

        // 对齐之后的长度
        // 第一个4 代表 2个字节的Type + 2个字节的Length本身，其后是对齐4的整数倍字节数的Value
        public ushort CompletionLength => (ushort)(StunAttributeType == StunAttributeType.Useless ? 0 : 4 + Length + (4 - Length % 4) % 4);

        public IStunAttributeValue StunAttributeValue { get; set; } = new UselessStunAttributeValue();

        /// <summary>
        /// 往buffer中写入StunAttribute
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int WriteTo(Span<byte> buffer)
        {
            var length = 4 + Length;
            var alignLength = (4 - Length % 4) % 4; // 为了对齐增加的字节数
            var completionLength = length + alignLength;    // = CompletionLength

            Requires.Range(buffer.Length >= completionLength, nameof(buffer));

            BinaryPrimitives.WriteUInt16BigEndian(buffer, (ushort)StunAttributeType);
            BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], Length);
            var valueLength = StunAttributeValue.WriteTo(buffer[4..]);

            Assumes.True(valueLength == Length);
            System.Security.Cryptography.RandomNumberGenerator.Fill(buffer.Slice(length, alignLength));

            return completionLength;
        }

        public int TryParse(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> magicCookieAndTransactionId)
        {

        }
    }
}
