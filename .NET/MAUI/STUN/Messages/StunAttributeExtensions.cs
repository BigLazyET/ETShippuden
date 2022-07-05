using STUN.Enums;
using STUN.Messages.StunAttributeValues;
using System.Net;

namespace STUN.Messages
{
    public static class StunAttributeExtensions
    {
        public static StunAttribute BuildChangeRequest(bool changeIp, bool changePort)
        {
            var stunAttribute = new StunAttribute
            {
                StunAttributeType = StunAttributeType.ChangeRequest,
                Length = 4,
                StunAttributeValue = new ChangeRequestStunAttributeValue
                {
                    ChangeIp = changeIp,
                    ChangePort = changePort
                }
            };

            return stunAttribute;
        }

        public static IPEndPoint? GetIPEndPointFromMappedAddressAttribute(this StunMessage5389 stunMessage)
        {
            var mappedAddressAttribute = stunMessage.Attributes.FirstOrDefault(a => a.StunAttributeType == StunAttributeType.MappedAddress);

            if (mappedAddressAttribute == null)
                return null;

            var mappedAddressAttributeValue = (MappedAddressStunAttributeValue)mappedAddressAttribute.StunAttributeValue;
            return new IPEndPoint(mappedAddressAttributeValue.Address!, mappedAddressAttributeValue.Port);
        }

        public static IPEndPoint? GetIPEndPointFromChangedAddressAttribute(this StunMessage5389 stunMessage)
        {
            var changedAddressAttribute = stunMessage.Attributes.First(a => a.StunAttributeType == StunAttributeType.ChangedAddress);

            if (changedAddressAttribute is null)
                return null;

            var changedAddressAttributeValue = (ChangedAddressStunAttributeValue)changedAddressAttribute.StunAttributeValue;
            return new IPEndPoint(changedAddressAttributeValue.Address!, changedAddressAttributeValue.Port);
        }
    }
}
