
namespace Dora.Xamarin.WebView.Common.Bridges
{
    public class CallJsMessage
    {
        /// <summary>
        /// 调用js方法|callback方法所需要的参数的序列化值
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Js 方法名 - 真实是存放于前端events字典中的key，可以理解为js方法名
        /// 包括两个方面：
        /// 1. js 方法
        /// 2. js callback方法
        /// </summary>
        public string MethodName { get; set; }

        public int Timer { get; set; }
    }
}
