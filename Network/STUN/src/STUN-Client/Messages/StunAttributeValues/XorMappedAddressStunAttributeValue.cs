using Microsoft;
using System.Buffers.Binary;
using System.Net;

namespace STUN.Messages.StunAttributeValues
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5389#section-15.2
    /// STUN服务器将源传输地址复制到STUN Binding响应中XOR-MAPPED-ADDRESS属性中，并将绑定响应发送回STUN客户端
    /// 当这个数据包通过NAT返回时，NAT将修改IP报头中的目的传输地址，但是STUN响应主体中XOR-MAPPED-ADDRESS属性中的传输地址将保持不变
    /// 通过这种方式，客户端可以了解最外面的NAT相对于STUN服务器分配的反射传输地址
    /// 客户端将响应中发来的IP地址和端口与其发送的IP地址和端口进行比较，以此来判断客户端和服务器之间有没有NAT
    /// 
    /// </summary>
    public class XorMappedAddressStunAttributeValue : AddressStunAttributeValue
    {
        private readonly byte[] _magicCookieAndTransactinId;

        public XorMappedAddressStunAttributeValue(ReadOnlySpan<byte> magicCookieAndTransactinId)
        {
            Requires.Argument(magicCookieAndTransactinId.Length == 16, nameof(magicCookieAndTransactinId), "Wrong transaction ID length");
            _magicCookieAndTransactinId = magicCookieAndTransactinId.ToArray();
        }

        public override int WriteTo(Span<byte> buffer)
        {
            Verify.Operation(Address is not null, "Address info should be set");
            Requires.Range(buffer.Length >= 4 + 4, nameof(buffer));

            buffer[0] = 0;
            buffer[1] = (byte)IpFamily;
            BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], Xor(Port));
            if (!Xor(Address).TryWriteBytes(buffer[4..], out var bytesWritten))
                throw new ArgumentOutOfRangeException(nameof(buffer));
            return 4 + bytesWritten;
        }

        public override bool TryParse(ReadOnlySpan<byte> buffer)
        {
            if (!base.TryParse(buffer))
                return false;

            Assumes.NotNull(Address);

            Port = Xor(Port);
            Address = Xor(Address);

            return true;
        }

        private ushort Xor(ushort port)
        {
            Span<byte> span = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(span, port);
            span[0] ^= _magicCookieAndTransactinId[0];  // 异或
            span[1] ^= _magicCookieAndTransactinId[1];
            return BinaryPrimitives.ReadUInt16BigEndian(span);
        }

        private IPAddress Xor(IPAddress address)
        {
            Span<byte> span = stackalloc byte[16];
            Assumes.True(address.TryWriteBytes(span, out int bytesWritten));

            for (int i = 0; i < bytesWritten; ++i)
            {
                span[i] ^= _magicCookieAndTransactinId[i];
            }

            return new IPAddress(span[..bytesWritten]);
        }
    }
}
