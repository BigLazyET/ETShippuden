using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;
using System.Net;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NatCheck5389Outcome : ObservableObject
    {
        private NatType _natType;
        private BindingTestResult _bindingTest;
        private MappingBehavior _mappingBehavior;
        private FilteringBehavior _filteringBehavior;
        private IPEndPoint _localIPEndPoint;
        private IPEndPoint _publicIPEndPoint;

        /// <summary>
        /// NAT类型
        /// </summary>
        public NatType NatType { get => _natType; set => SetProperty(ref _natType, value); }

        /// <summary>
        /// 对StunServer的BindingRequest结果
        /// </summary>
        public BindingTestResult BindingTest { get => _bindingTest; set => SetProperty(ref _bindingTest, value); }

        /// <summary>
        /// NAT映射规则
        /// </summary>
        public MappingBehavior MappingBehavior { get => _mappingBehavior; set => SetProperty(ref _mappingBehavior, value); }

        /// <summary>
        /// NAT过滤规则
        /// </summary>
        public FilteringBehavior FilteringBehavior { get => _filteringBehavior; set => SetProperty(ref _filteringBehavior, value); }

        /// <summary>
        /// 本地IPEndPoint
        /// </summary>
        public IPEndPoint LocalIPEndPoint { get { return _localIPEndPoint; } set { SetProperty(ref _localIPEndPoint, value); } }

        /// <summary>
        /// NAT映射过后的公网IP
        /// </summary>
        public IPEndPoint PublicIPEndPoint { get => _publicIPEndPoint; set => SetProperty(ref _publicIPEndPoint, value); }
    }
}
