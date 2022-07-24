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
            var changedAddressAttribute = stunMessage.Attributes.FirstOrDefault(a => a.StunAttributeType == StunAttributeType.ChangedAddress);

            if (changedAddressAttribute is null)
                return null;

            var changedAddressAttributeValue = (ChangedAddressStunAttributeValue)changedAddressAttribute.StunAttributeValue;
            return new IPEndPoint(changedAddressAttributeValue.Address!, changedAddressAttributeValue.Port);
        }

        public static IPEndPoint? GetIPEndPointFromXorMappedAddressAttribute(this StunMessage5389 stunMessage)
        {
            var xorMappedAddressAttribute = stunMessage.Attributes.FirstOrDefault(a => a.StunAttributeType == StunAttributeType.XorMappedAddress) ??
                stunMessage.Attributes.FirstOrDefault(a => a.StunAttributeType == StunAttributeType.MappedAddress);

            if (xorMappedAddressAttribute is null)
                return null;

            var xorMappedAddressAttributeValue = (XorMappedAddressStunAttributeValue)xorMappedAddressAttribute.StunAttributeValue;
            return new IPEndPoint(xorMappedAddressAttributeValue.Address!, xorMappedAddressAttributeValue.Port);
        }

        public static IPEndPoint GetStunOtherEndPoint(this StunMessage5389 stunMessage)
        {
            var addressAttribute = stunMessage.Attributes.FirstOrDefault(a => a.StunAttributeType == StunAttributeType.OtherAddress) ??
                stunMessage.Attributes.FirstOrDefault(a => a.StunAttributeType == StunAttributeType.ChangedAddress);

            if (addressAttribute is null)
                return null;

            var addressValue = (AddressStunAttributeValue)addressAttribute.StunAttributeValue;
            return new IPEndPoint(addressValue.Address!, addressValue.Port);
        }
    }
}
