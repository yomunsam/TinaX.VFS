using TinaXEditor.VFS.AssetBuilder.AssetBundles;
using TinaXEditor.VFS.Querier;

namespace TinaXEditor.VFS.AssetBuilder.Discoverer
{
    /// <summary>
    /// 资产发现者
    /// </summary>
    public interface IAssetDiscoverer
    {
        /// <summary>
        /// 去收集可被管理的资产，最终填到VFSAssetBundles对象里
        /// </summary>
        /// <param name="vfsAssetBundles"></param>
        /// <param name="assetQuerier">资产查询器</param>
        void DiscoverAssets(VFSAssetBundles vfsAssetBundles, IEditorAssetQuerier assetQuerier);
    }
}
