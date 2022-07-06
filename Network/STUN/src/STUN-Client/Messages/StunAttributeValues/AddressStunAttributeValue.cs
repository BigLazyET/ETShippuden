using Microsoft;
using STUN.Enums;
using System.Buffers.Binary;
using System.Net;

namespace STUN.Messages
{
    public class AddressStunAttributeValue : IStunAttributeValue
    {
        public IpFamily IpFamily { get; set; }

        public ushort Port { get; set; }

        public IPAddress? Address { get; set; }

        public virtual bool TryParse(ReadOnlySpan<byte> buffer)
        {
            var length = 4;
            if (buffer.Length < length)
                return false;

            IpFamily = (IpFamily)buffer[1];

            length = IpFamily switch
            {
                IpFamily.IPv4 => length + 4,
                IpFamily.IPv6 => length + 16,
                _ => throw new ArgumentException("ipfamily not recognize")
            };

            if (buffer.Length != length)
                return false;

            Port = BinaryPrimitives.ReadUInt16BigEndian(buffer[2..]);
            Address = new IPAddress(buffer[4..]);
            return true;
        }

        public virtual int WriteTo(Span<byte> buffer)
        {
            Verify.Operation(Address is not null, "You should set Address info");
            Requires.Range(buffer.Length > 4 + 4, nameof(buffer));

            buffer[0] = 0;
            buffer[1] = (byte)IpFamily;
            BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], Port);
            Requires.Range(Address.TryWriteBytes(buffer[4..], out int bytesWritten), nameof(buffer));

            return 4 + bytesWritten;
        }
    }
}
