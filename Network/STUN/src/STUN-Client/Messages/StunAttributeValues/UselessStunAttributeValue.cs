namespace STUN.Messages
{
    public class UselessStunAttributeValue : IStunAttributeValue
    {
        public bool TryParse(ReadOnlySpan<byte> buffer)
        {
            return true;
        }

        public int WriteTo(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }
    }
}
