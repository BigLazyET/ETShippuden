using Microsoft;
using STUN.Enums;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace STUN.Messages
{
    /// <summary>
    /// RFC 5389 协议对Stun message的规定：https://tools.ietf.org/html/rfc5389#section-6
    /// </summary>
    public class StunMessage5389
    {
        // Start with a 20-byte header
        public StunMessageHeader Header { get; set; }

        // Then followed by zero or more Attributes
        public IEnumerable<StunAttribute> Attributes { get; set; } = Array.Empty<StunAttribute>();

        public StunMessage5389()
        {
            RandomNumberGenerator.Fill(Header.TransactionId);
        }

        public int WriteTo(Span<byte> buffer)
        {
            var attributesLength = Attributes.Aggregate<StunAttribute, ushort>(0, (current, attribute) => (ushort)(current + attribute.CompletionLength));
            var length = 20 + attributesLength; // 20-bytes header
            Requires.Range(buffer.Length >= length, nameof(length));

            // Header
            BinaryPrimitives.WriteUInt16BigEndian(buffer, (ushort)Header.StunMessageType);
            BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], attributesLength);
            BinaryPrimitives.WriteUInt32BigEndian(buffer[4..], Header.MagicCookie);
            Header.TransactionId.CopyTo(buffer[8..]);

            // Attributes
            var attributesBuffer = buffer.Slice(20);
            foreach (var attribute in Attributes)
            {
                var outLength = attribute.WriteTo(attributesBuffer);
                attributesBuffer = attributesBuffer.Slice(outLength);
            }

            //buffer = buffer[20..];
            //foreach (var attribute in Attributes)
            //{
            //    var outLength = attribute.WriteTo(buffer);
            //    buffer = buffer[outLength..];
            //}

            return length;
        }

        public bool TryParse(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 20)
                return false;

            // Header
            Span<byte> timeSpan = stackalloc byte[2];
            timeSpan[0] = (byte)(buffer[0] & 0b0011_1111);
            timeSpan[1] = buffer[1];
            var type = (StunMessageType)BinaryPrimitives.ReadUInt16BigEndian(timeSpan);

            if (!Enum.IsDefined(typeof(StunMessageType), type))
                return false;

            Header.StunMessageType = type;
            ushort length = BinaryPrimitives.ReadUInt16BigEndian(buffer[2..]);
            Header.MagicCookie = BinaryPrimitives.ReadUInt32BigEndian(buffer[4..]);
            buffer.Slice(8, 12).CopyTo(Header.TransactionId);

            if (buffer.Length != length + 20)
                return false;

            // Attributes
            var attributes = new List<StunAttribute>();
            var attributesBuffer = buffer[20..];    // buffer.Slice(20);
            var magicCookieAndTransactionId = buffer.Slice(4, 16);

            while (attributesBuffer.Length > 0)
            {
                var attribute = new StunAttribute();
                var offset = attribute.TryParse(attributesBuffer, magicCookieAndTransactionId);
                if (offset > 0)
                {
                    attributes.Add(attribute);
                    attributesBuffer = attributesBuffer[offset..];
                }
                else
                {
                    Console.WriteLine($"[Warning] Ignore wrong attribute: {Convert.ToHexString(attributesBuffer)}");
                    break;
                }
            }

            Attributes = attributes;
            return true;
        }

        public bool IsSameTransaction(StunMessage5389 other)
        {
            return Header.MagicCookie == other.Header.MagicCookie && Header.TransactionId.AsSpan().SequenceEqual(other.Header.TransactionId);
        }
    }
}
