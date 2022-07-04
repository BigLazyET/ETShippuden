using Microsoft;
using System.Text;

namespace STUN.Messages.StunAttributeValues
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5389#section-15.6
    /// </summary>
    public class ErrorCodeStunAttributeValue : IStunAttributeValue
    {
        const int MaxReasonPhraseBytesLength = 762;

        public ushort ErrorCode { get; set; }

        public string ReasonPhrase { get; set; } = string.Empty;

        public byte StunMessageClass => (byte)(ErrorCode % 1000 / 100); // request/response(success,error)/indication

        public byte Number => (byte)(ErrorCode % 100);

        public bool TryParse(ReadOnlySpan<byte> buffer)
        {
            //if (buffer.Length < 4 || buffer.Length > 4 + MaxReasonPhraseBytesLength)
            //    return false;
            if (buffer.Length is < 4 or > 4 + MaxReasonPhraseBytesLength)
                return false;

            //StunMessageClass = buffer[2];
            //Number = buffer[3]
            var stunMessageClass = (byte)(buffer[2] & 0b111);
            var number = Math.Min(buffer[3], (ushort)99);
            ErrorCode = (ushort)(stunMessageClass * 100 + number);
            ReasonPhrase = Encoding.UTF8.GetString(buffer[4..]);

            return true;
        }

        public int WriteTo(Span<byte> buffer)
        {
            Requires.Range(buffer.Length >= 4, nameof(buffer));

            buffer[0] = buffer[1] = 0;
            buffer[2] = StunMessageClass;
            buffer[3] = Number;

            var length = Encoding.UTF8.GetBytes(ReasonPhrase, buffer[4..]);
            return 4 + Math.Min(length, MaxReasonPhraseBytesLength);
        }
    }
}
