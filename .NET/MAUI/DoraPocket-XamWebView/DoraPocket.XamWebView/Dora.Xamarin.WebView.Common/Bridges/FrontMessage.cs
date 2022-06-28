using System.Runtime.Serialization;

namespace Dora.Xamarin.WebView.Common.Bridges
{
    /// <summary>
    /// 接收到的js调用native传递过来的信息
    /// </summary>
    [DataContract]
    public class FrontMessage
    {
        /// <summary>
        /// js要调用的native action名
        /// </summary>
        [DataMember(Name ="a")]
        public string ActionName { get; set; }

        /// <summary>
        /// js的回调方法名，准备的说是存在前端events字典中的key: callbackIdx
        /// </summary>
        [DataMember(Name = "m")]
        public string MethodName { get; set; }

        /// <summary>
        /// 是否有js回调
        /// </summary>
        [DataMember(Name = "h")]
        public bool HasCallback { get; set; }

        /// <summary>
        /// 回调是否需要缓存
        /// </summary>
        [DataMember(Name = "cache")]
        public bool Cache { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        [DataMember(Name = "timer")]
        public int Timer { get; set; }

        /// <summary>
        /// native action方法所需要的参数
        /// </summary>
        [DataMember(Name = "d")]
        public object Data { get; set; }
    }
}
