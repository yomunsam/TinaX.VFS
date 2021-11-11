using System.Threading.Tasks;
using UnityEditor;

namespace TinaXEditor.VFS.AssetBuilder.Discoverer
{
    /// <summary>
    /// 资产发现者
    /// </summary>
    public interface IAssetDiscoverer
    {
        /// <summary>
        /// 是否执行过至少一次收集任务
        /// </summary>
        bool Collected { get; }

        /// <summary>
        /// 收集可被管理的资产
        /// </summary>
        /// <returns></returns>
        Task CollectManageableAssetsAsync();
        Task<AssetBundleBuild[]> GetUnityAssetBundleBuilds();
    }
}
