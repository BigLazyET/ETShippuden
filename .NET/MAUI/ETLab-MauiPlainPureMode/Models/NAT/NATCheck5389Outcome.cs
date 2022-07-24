using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;
using System.Net;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck5389Outcome : ObservableObject
    {
        private NATType _natType;
        private BindingTestResult _bindingTest;
        private MappingBehavior _mappingBehavior;
        private FilteringBehavior _filteringBehavior;
        private IPEndPoint _localIPEndPoint;
        private IPEndPoint _publicIPEndPoint;

        public NATType NATType { get => _natType; set => SetProperty(ref _natType, value); }

        public BindingTestResult BindingTest { get => _bindingTest; set => SetProperty(ref _bindingTest, value); }

        public MappingBehavior MappingBehavior { get => _mappingBehavior; set => SetProperty(ref _mappingBehavior, value); }

        public FilteringBehavior FilteringBehavior { get => _filteringBehavior; set => SetProperty(ref _filteringBehavior, value); }

        public IPEndPoint LocalIPEndPoint { get { return _localIPEndPoint; } set { SetProperty(ref _localIPEndPoint, value); } }

        public IPEndPoint PublicIPEndPoint { get => _publicIPEndPoint; set => SetProperty(ref _publicIPEndPoint, value); }
    }
}
