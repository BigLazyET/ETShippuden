
namespace Dora.Xamarin.WebView.Common.Interfaces
{
    /// <summary>
    /// 文件资源路径处理器
    /// 目前先确定，后期可以通过配置json来动态设置相应的路径！
    /// </summary>
    public interface IResourcePathResolver
    {
        /// <summary>
        /// 基础路径-绝对路径，一切文件都放在documenet为基础路径的自定义文件夹下面
        /// </summary>
        public string DocumentsDirectory { get; }

        /// <summary>
        /// 获取hybrid zip解压后的基础路径-绝对路径
        /// 形如：xxx/xxx/Hybrid
        /// </summary>
        /// <returns></returns>
        public string GetExtractedHybridBasePath();

        /// <summary>
        /// 获取hybrid zip解压后运用版本的基础路径-绝对路径
        /// 形如：xxx/xxx/Hybrid/{版本}
        /// </summary>
        /// <returns></returns>
        public string GetExtractedHybridVersionPath();

    }
}
