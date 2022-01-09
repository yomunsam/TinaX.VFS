using TinaX.VFS.Assets;
using TinaX.VFS.Querier;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset
{
    /// <summary>
    /// 加载资产 上下文
    /// </summary>
    public class LoadAssetContext
    {
        public LoadAssetContext(AssetQuerier assetQuerier, VFSAssetManager assetManager)
        {
            this.AssetQuerier = assetQuerier;
            this.AssetManager = assetManager;
        }

        /// <summary>
        /// 是否终断Pipeline的标记
        /// </summary>
        public bool BreakPipeline { get; set; } = false;

        /// <summary>
        /// 汉语Log
        /// </summary>
        public bool HansLog { get; set; } = false;

        public void Break() => BreakPipeline = true;



        public AssetQuerier AssetQuerier { get; }
        public VFSAssetManager AssetManager { get; }
    }
}
