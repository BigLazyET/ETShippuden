using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;
using System.Net;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck3489Outcome : ObservableObject
    {
        private NatType _natType = NatType.Unknown;
        private IPEndPoint _localIPEndPoint;
        private IPEndPoint _publicIPEndPoint;
        private IPEndPoint _actualLocalIPEndPoint;

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

        /// <summary>
        /// 实际本地发送请求的地址
        /// 可以理解为StunServer Response中实际返回的
        /// </summary>
        public IPEndPoint ActualLocalIPEndPoint
        {
            get
            {
                return _actualLocalIPEndPoint;
            }
            set
            {
                SetProperty(ref _actualLocalIPEndPoint, value);
            }
        }
    }
}
