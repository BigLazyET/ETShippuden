using ETLab_MauiPlainPureMode.ViewModels;
using STUN.Enums;
using System.Net;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck3489Outcome : ObservableObject
    {
        private NATType _natType;
        /// <summary>
        /// 检测的NAT类型
        /// </summary>
        public NATType NATTYPE { get => _natType; set => SetProperty(ref _natType, value); }

        private IPEndPoint _localIPEndPoint;
        /// <summary>
        /// 本地发送请求的地址(IP:Port)
        /// </summary>
        public IPEndPoint LocalIPEndPoint { get => _localIPEndPoint; set => SetProperty(ref _localIPEndPoint, value); }

        private IPEndPoint _publicIPEndPoint;
        /// <summary>
        /// 本地地址通过NAT映射在公网的地址(IP:Port)
        /// </summary>
        public IPEndPoint PublicIPEndPoint { get => _publicIPEndPoint; set => SetProperty(ref _publicIPEndPoint, value); }
    }
}
