using System.Threading;
using Cysharp.Threading.Tasks;

namespace TinaX.VFS.Pipelines.LoadVFSConfigAsset
{
    /// <summary>
    /// 加载 VFS 配置资产的处理者 接口
    /// </summary>
    public interface ILoadVFSConfigAssetHandler
    {
        string HandlerName { get; }

        /// <summary>
        /// 异步加载VFS配置资产
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask LoadVFSConfigAssetAsync(LoadVFSConfigAssetContext context, ILoadVFSConfigAssetHandler next, CancellationToken cancellationToken);
    }
}
