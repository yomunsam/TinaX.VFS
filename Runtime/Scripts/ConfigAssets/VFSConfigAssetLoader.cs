using System.Threading;
using Cysharp.Threading.Tasks;
using TinaX.Systems.Pipeline;
using TinaX.VFS.Extensions;
using TinaX.VFS.Pipelines.LoadVFSConfigAsset;

namespace TinaX.VFS.ConfigAssets
{
    /// <summary>
    /// VFS配置资产 加载器
    /// </summary>
    public class VFSConfigAssetLoader
    {
        private readonly XPipeline<ILoadVFSConfigAssetHandler> m_LoadVFSConfigAssetAsyncPipeline = new XPipeline<ILoadVFSConfigAssetHandler>();

        public VFSConfigAssetLoader()
        {
            LoadVFSConfigAssetAsyncPipelineConfigure(ref m_LoadVFSConfigAssetAsyncPipeline);
        }


        private void LoadVFSConfigAssetAsyncPipelineConfigure(ref XPipeline<ILoadVFSConfigAssetHandler> pipeline)
        {
            //准备从PersistentDataPath加载
            pipeline.Use(LoadVFSConfigAssetHandlerNameConsts.ReadyLoadFromPersistentDataPath, (LoadVFSConfigAssetContext context, ILoadVFSConfigAssetHandler next, CancellationToken cancellationToken) =>
            {
                return UniTask.CompletedTask;
            });
        }
    }
}
