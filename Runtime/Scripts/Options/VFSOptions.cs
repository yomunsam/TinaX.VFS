using TinaX.VFS.ConfigAssets.Loader;
using TinaX.VFS.Consts;

namespace TinaX.VFS.Options
{
    /// <summary>
    /// VFS 系统配置
    /// </summary>
    public class VFSOptions
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 实现内置资产服务接口
        /// </summary>
        public bool ImplementBuiltInAssetServiceInterface { get; set; } = true;

        public string ConfigAssetLoadPath { get; set; } = VFSConsts.DefaultConfigAssetName;


    }
}
