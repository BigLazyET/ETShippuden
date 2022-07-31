using STUN.Enums;
using STUN.Messages;
using STUN.Proxy;
using System.Net;

namespace STUN.Client
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5389#section-7.2.1
    /// https://tools.ietf.org/html/rfc5780#section-4.2
    /// https://cloud.tencent.com/developer/article/1760086 
    /// http://www.52im.net/thread-542-1-1.html
    /// http://www.52im.net/thread-2872-1-1.html
    /// RFC5389协议将映射规则(Mapping behavior)和过滤规则(Filtering behavior)分开定义，各自有三种，总计有9种组合
    /// Mapping指的是内网IP映射成外网IP的这个Map；Filtering指的是NAT判断并过滤外网发过来的包能否通过(Mapping映射过的外网IP)发给内网IP
    /// 可以五步来确认映射规则和过滤规则，先确认映射规则，再确认过滤规则；前者主要通过bind request，后者主要通过changerequest来确认
    /// </summary>
    public class StunClient5389 : StunClient
    {
        public override StunResult5389 StunResult => new ();

        public StunClient5389(IPEndPoint remoteEndPoint, IUdpProxy udpProxy, TimeSpan receiveTimeout)
            : base(remoteEndPoint, udpProxy, receiveTimeout) { }

        public override async ValueTask QueryAsync(CancellationToken cancellationToken = default)
        {
            // Pre Binding Request Check
            await PreBindingRequestCheckAsync();
            if(StunResult.BindingTestResult != BindingTestResult.Success)
                return;

            // Mapping Behavior Check
            var isMappingCarryOn = await IsCarryOnByMappingCheck1Async(cancellationToken);

            if (isMappingCarryOn)
                isMappingCarryOn = await IsCarryOnByMappingCheck2Async(cancellationToken);

            if (isMappingCarryOn)
                isMappingCarryOn = await IsCarryOnByMappingCheck3Async(cancellationToken);

            if (isMappingCarryOn)
                StunResult.MappingBehavior = MappingBehavior.Unknown;

            // Filtering Behavior Check
            var isFilteringCarryOn = await IsCarryOnByFilteringCheck1Async(cancellationToken);

            if (isFilteringCarryOn)
                isFilteringCarryOn = await IsCarryOnByFilteringCheck2Async(cancellationToken);

            if (isFilteringCarryOn)
                StunResult.FilteringBehavior = FilteringBehavior.Unknown;


            // 将NAT映射规则和过滤规则组合起来就形成9中不同的NAT行为类型，而RFC3489只描述了9种NAT组合行为类型中的以下4种：
            // 1）Endpoint Independent Mapping和Endpoint - Independent Filtering组合对应于RFC3489中的Full Cone NAT；
            // 2）Endpoint Independent Mapping和Address - Dependent Filtering组合对应于RFC3489中的Restricted Cone NAT；
            // 3）Endpoint Independent Mapping和Address and Port - Dependent Filtering组合对应于RFC3489中的Port Restricted Cone NAT；
            // 4）Address and Port - Dependent Mapping和Address and Port - Dependent Filtering组合是RFC3489中所说的Symmetric NAT。

            var mappingBehavior = StunResult.MappingBehavior;
            var filteringBehavior = StunResult.FilteringBehavior;
            if (mappingBehavior == MappingBehavior.EndPointIndependent && filteringBehavior == FilteringBehavior.EndPointIndependent)
                StunResult.NATType = NatType.FullCone;
            if (mappingBehavior == MappingBehavior.EndPointIndependent && filteringBehavior == FilteringBehavior.AddressDependent)
                StunResult.NATType = NatType.RestrictedCone;
            if (mappingBehavior == MappingBehavior.EndPointIndependent && filteringBehavior == FilteringBehavior.AddressAndPortDependent)
                StunResult.NATType = NatType.PortRestrictedCone;
            if (mappingBehavior == MappingBehavior.AddressAndPortDependent && filteringBehavior == FilteringBehavior.AddressAndPortDependent)
                StunResult.NATType = NatType.Symmetric;
        }

        #region Pre-BindingRequest Check
        private async Task PreBindingRequestCheckAsync(CancellationToken cancellationToken = default)
        {
            var request = new StunMessage5389();

            var response = await RequestAsync(request, RemoteEndPoint, RemoteEndPoint, cancellationToken);
            var mappedEndPoint = response.stunMessage.GetIPEndPointFromXorMappedAddressAttribute();
            var stunOtherEndPoint = response.stunMessage.GetStunOtherEndPoint();

            if (response is null)
                StunResult.BindingTestResult = BindingTestResult.Fail;
            if (mappedEndPoint is null)
                StunResult.BindingTestResult = BindingTestResult.UnSupportedServer;
            if (stunOtherEndPoint is not null && stunOtherEndPoint.Address.Equals(RemoteEndPoint.Address) && stunOtherEndPoint.Port == RemoteEndPoint.Port)
                StunResult.BindingTestResult = BindingTestResult.UnSupportedServer;

            StunResult.BindingTestResult = BindingTestResult.Success;
        }
        #endregion

        #region Mapping Behavior Check
        /// <summary>
        /// 客户端A以IP_CA: PORT_CA给STUN Server的IP_SA: PORT_SA发送一个bind请求
        /// STUN server以IP_SA: PORT_SA给客户端A的IP_CA: PORT_CA回复响应，响应内容大体为：
        /// (NAT映射后的IP地址和端口为：IP_MCA1: PORT_MCA1，STUN Server的另外一个IP地址和端口为：IP_SB: PORT_SB)
        /// 这个时候客户端判断，如果IP_CA: PORT_CA == IP_MCA1: PORT_MCA1，那么该客户端是拥有公网IP的，NAT类型侦测结束
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>isCarryOn：true/侦测继续，false/侦测到此结束</returns>
        private async ValueTask<bool> IsCarryOnByMappingCheck1Async(CancellationToken cancellationToken = default)
        {
            var request = new StunMessage5389();    // the default of StunMessageType is BindingRequest
            var response = await RequestAsync(request, RemoteEndPoint, RemoteEndPoint, cancellationToken);
            var mappedEndPoint = response?.stunMessage.GetIPEndPointFromXorMappedAddressAttribute();
            var stunOtherEndPoint = response?.stunMessage.GetStunOtherEndPoint();

            if (response is not null && mappedEndPoint is not null)
            {
                // if (Equals(mappedEndPoint.Address, response.localAddress) && mappedEndPoint.Port == localIPEndPoint.Port)
                if (Equals(mappedEndPoint.Address, LocalEndPoint.Address) && mappedEndPoint.Port == LocalEndPoint.Port)
                {
                    StunResult.MappingBehavior = MappingBehavior.Direct;
                    StunResult.PublicEndPoint = mappedEndPoint;
                    StunResult.OtherEndPoint = stunOtherEndPoint;
                    return false;
                }
            }

            StunResult.PublicEndPoint = mappedEndPoint;
            StunResult.OtherEndPoint = stunOtherEndPoint;

            return true;
        }

        /// <summary>
        /// 客户端A以IP_CA: PORT_CA给STUN server的IP_SB: PORT_SA(相对步骤1 ip改变了)发送一个bind请求
        /// STUN server以IP_SB: PORT_SA给客户端A的IP_CA: PORT_CA回复响应，响应内容大体为：(NAT映射后的IP地址和端口为：IP_MCA2: PORT_MCA2)
        /// 这个时候客户端判断，如果IP_MCA1: PORT_MCA1 == IP_MCA2: PORT_MCA2，那么NAT是Endpoint Independent Mapping的映射规则
        /// 也就是同样的内网地址IP_CA: PORT_CA经过这种NAT映射后的IP_M: PORT_M是固定不变的；
        /// 如果IP_MCA1: PORT_MCA1 != IP_MCA2: PORT_MCA2,那么就要进行下面的第3步测试
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>isCarryOn：true/侦测继续，false/侦测到此结束</returns>
        private async ValueTask<bool> IsCarryOnByMappingCheck2Async(CancellationToken cancellationToken = default)
        {
            var request = new StunMessage5389();

            var remoteIPEndPoint = new IPEndPoint(StunResult.OtherEndPoint!.Address, RemoteEndPoint.Port);
            var response = await RequestAsync(request, remoteIPEndPoint, remoteIPEndPoint, cancellationToken);
            var mappedEndPoint = response.stunMessage.GetIPEndPointFromXorMappedAddressAttribute();

            if (Equals(mappedEndPoint.Address, StunResult.PublicEndPoint.Address) && mappedEndPoint.Port == StunResult.PublicEndPoint.Port)
            {
                StunResult.MappingBehavior = MappingBehavior.EndPointIndependent;
                StunResult.PublicEndPoint = mappedEndPoint;
                return false;
            }

            StunResult.PublicEndPoint = mappedEndPoint;
            return true;
        }

        /// <summary>
        /// 客户端A以IP_CA: PORT_CA给STUN server的IP_SB: PORT_SB(相对步骤1 ip和port改变了，相对步骤2 port改变了)发送一个bind请求
        /// STUN server以IP_SB: PORT_SB给客户端A的IP_CA: PORT_CA回复响应，响应内容大体为：（NAT映射后的IP地址和端口为：IP_MCA3: PORT_MCA3）
        /// 这个时候客户端判断，如果IP_MCA2: PORT_MCA2== IP_MCA3: PORT_MCA3，那么NAT是Address Dependent Mapping的映射规则
        /// 也就是只要是目的IP是相同的，那么同样的内网地址IP_CA: PORT_CA经过这种NAT映射后的IP_M: PORT_M是固定不变的；
        /// 如果IP_MCA2: PORT_MCA2!= IP_MCA3: PORT_MCA3，那么NAT是Address and Port Dependent Mapping
        /// 只要目的IP和PORT中有一个不一样，那么同样的内网地址IP_CA: PORT_CA经过这种NAT映射后的IP_M: PORT_M是不一样的
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>isCarryOn：true/侦测继续，false/侦测到此结束</returns>
        private async ValueTask<bool> IsCarryOnByMappingCheck3Async(CancellationToken cancellationToken = default)
        {
            var request = new StunMessage5389();

            var response = await RequestAsync(request, StunResult.OtherEndPoint!, StunResult.OtherEndPoint!, cancellationToken);
            var mappedEndPoint = response.stunMessage.GetIPEndPointFromXorMappedAddressAttribute();

            if (Equals(StunResult.PublicEndPoint.Address, mappedEndPoint.Address) && StunResult.PublicEndPoint.Port == mappedEndPoint.Port)
            {
                StunResult.MappingBehavior = MappingBehavior.AddressDependent;
                StunResult.PublicEndPoint = mappedEndPoint;
                return false;
            }
            if (!Equals(StunResult.PublicEndPoint.Address, mappedEndPoint.Address) && StunResult.PublicEndPoint.Port != mappedEndPoint.Port)
            {
                StunResult.MappingBehavior = MappingBehavior.AddressAndPortDependent;
                StunResult.PublicEndPoint = mappedEndPoint;
                return false;
            }

            StunResult.PublicEndPoint = mappedEndPoint;
            return true;
        }
        #endregion

        #region Filtering Behavior Check
        /// <summary>
        /// 客户端A以IP_CA: PORT_CA给STUN server的IP_SA: PORT_SA发送一个bind请求(请求中带CHANGE-REQUEST attribute来要求stun server改变IP和PORT来响应)
        /// STUN server以IP_SB: PORT_SB给客户端A的IP_CA: PORT_CA回复响应。
        /// 如果客户端A能收到STUN server的响应，那么NAT是Endpoint-Independent Filtering的过滤规则，
        /// 也就是只要给客户端A的IP_CA: PORT_CA映射后的IP_MCA: PORT_MCA地址发送数据都能通过NAT到达客户端A的IP_CA: PORT_CA（这种过滤规则的NAT估计很少）
        /// 如果不能收到STUN server的响应，那么需要进行下一步测试
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async ValueTask<bool> IsCarryOnByFilteringCheck1Async(CancellationToken cancellationToken = default)
        {
            var request = new StunMessage5389
            {
                Attributes = new[] { StunAttributeExtensions.BuildChangeRequest(true, true) }
            };

            var response = await RequestAsync(request, RemoteEndPoint, StunResult.OtherEndPoint, cancellationToken);
            if (response is not null)
            {
                StunResult.FilteringBehavior = Equals(response.remote, StunResult.OtherEndPoint) ? FilteringBehavior.EndPointIndependent
                    : FilteringBehavior.UnSupportedServer;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 客户端A以IP_CA: PORT_CA给STUN server的IP_SA: PORT_SA发送一个bind请求（请求中带CHANGE-REQUEST attribute来要求stun server改变PORT来响应）
        /// STUN server以IP_SA: PORT_SB给客户端A的IP_CA: PORT_CA回复响应。
        /// 如果客户端A能收到STUN server的响应，NAT是Address-Dependent Filtering的过滤规则，
        /// 也就是只要之前客户端A以IP_CA: PORT_CA给IP为IP_D的主机发送过数据，
        /// 那么在NAT映射的有效期内，IP为IP_D的主机以任何端口给客户端A的IP_CA: PORT_CA映射后的IP_MCA: PORT_MCA地址发送数据都能通过NAT到达客户端A的IP_CA: PORT_CA。
        /// 如果不能收到响应，NAT是Address and Port-Dependent Filtering的过滤规则，
        /// 也即是只有之前客户端A以IP_CA: PORT_CA给目的主机的IP_D: PORT_D发送过数据，
        /// 那么在NAT映射的有效期内，只有以IP_D: PORT_D给客户端A的IP_CA: PORT_CA映射后的IP_MCA: PORT_MCA地址发送数据才能通过NAT到达客户端A的IP_CA: PORT_CA
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async ValueTask<bool> IsCarryOnByFilteringCheck2Async(CancellationToken cancellationToken = default)
        {
            var request = new StunMessage5389
            {
                Attributes = new[] { StunAttributeExtensions.BuildChangeRequest(false, true) }
            };

            var receiveEndPoint = new IPEndPoint(RemoteEndPoint.Address, StunResult.OtherEndPoint.Port);
            var response = await RequestAsync(request, RemoteEndPoint, receiveEndPoint, cancellationToken);

            if (response is null)
            {
                StunResult.FilteringBehavior = FilteringBehavior.AddressAndPortDependent;
                return true;
            }

            StunResult.FilteringBehavior = (Equals(response.remote.Address, RemoteEndPoint.Address) && response.remote.Port != RemoteEndPoint.Port) ?
                    FilteringBehavior.AddressDependent : FilteringBehavior.UnSupportedServer;
            return false;
        }
        #endregion
    }
}
