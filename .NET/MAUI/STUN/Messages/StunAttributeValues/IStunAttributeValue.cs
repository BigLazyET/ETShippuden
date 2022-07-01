namespace STUN.Messages
{
    public interface IStunAttributeValue
    {
        /// <summary>
        /// 往buffer中写入StunAttributeValue
        /// 初始第一个字节写入 0
        /// 初始第二个字节写入 IpFamily
        /// 紧接两个字节写入   Port
        /// 剩下的字节写入     IPAddress
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        int WriteTo(Span<byte> buffer);

        /// <summary>
        /// 从buffer中独取并解析出StunAttributeValue
        /// 第二个字节解析出    IpFamily
        /// 紧接两个字节解析出  Port
        /// 剩下的字节写解析出  IPAddress
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        bool TryParse(ReadOnlySpan<byte> buffer);
    }
}
