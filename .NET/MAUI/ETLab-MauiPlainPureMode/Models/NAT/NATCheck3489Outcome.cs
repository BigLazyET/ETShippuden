using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;
using System.Net;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck3489Outcome : ObservableObject
    {
        private NatType _natType = NatType.Unknown;
        private IPEndPoint _localIPEndPoint; // new IPEndPoint(IPAddress.Any, 0); //new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
        private IPEndPoint _publicIPEndPoint;

        /// <summary>
        /// 检测的NAT类型
        /// </summary>
        public NatType NATTYPE { get => _natType; set => SetProperty(ref _natType, value); }

        /// <summary>
        /// 本地发送请求的地址(IP:Port)
        /// </summary>
        public IPEndPoint LocalIPEndPoint { get => _localIPEndPoint; set => SetProperty(ref _localIPEndPoint, value); }

        /// <summary>
        /// 本地地址通过NAT映射在公网的地址(IP:Port)
        /// </summary>
        public IPEndPoint PublicIPEndPoint { get => _publicIPEndPoint; set => SetProperty(ref _publicIPEndPoint, value); }
    }
}
