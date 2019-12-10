

namespace TinaX.VFSKit
{

    /// <summary>
    /// 资源路径
    /// </summary>
    internal struct AssetsPath
    {
        /// <summary>
        /// 可被系统IO类直接访问到的真实路径
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 在VFS中可访问到的虚拟路径
        /// </summary>
        public string VFSPath { get; set; }
    }
}
