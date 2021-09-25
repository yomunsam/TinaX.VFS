using TinaX.VFS.ConfigAssets.Loader;
using TinaX.VFS.Const;

namespace TinaX.VFS.Options
{
    /// <summary>
    /// VFS 系统配置
    /// </summary>
    public class VFSOption
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }

        public string ConfigAssetLoadPath { get; set; } = VFSConsts.DefaultConfigAssetName;


        public VFSConfigAssetLoader ConfigAssetLoader { get; } = new VFSConfigAssetLoader();
    }
}
