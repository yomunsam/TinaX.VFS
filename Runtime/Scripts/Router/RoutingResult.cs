using TinaX.VFS.Packages;

namespace TinaX.VFS.Router
{
    /// <summary>
    /// 路由结果
    /// </summary>
    public struct RoutingResult
    {
        /// <summary>
        /// 传入的加载路径
        /// </summary>
        public string LoadPath { get; set; }

        /// <summary>
        /// 加载的资产地址
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// 资产所属的Package
        /// </summary>
        public VFSPackage Package { get; set; }
    }
}
