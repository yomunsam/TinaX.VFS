using System;
using Cysharp.Threading.Tasks;
using TinaX.VFS.Assets;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset.Handlers
{
    /// <summary>
    /// 检查已加载的资产
    /// </summary>
    public class CheckLoadedAssetHandler : ILoadAssetHandler, ILoadAssetAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.CheckLoadedAsset;

        public void LoadAsset(ref LoadAssetContext context, ref LoadAssetPayload payload)
        {
            if (context.AssetManager.TryGet(payload.QueryResult!.Value.VirtualAssetPath, out VFSAsset asset))
            {
                payload.LoadedAsset = asset;
                context.Break(); //不需要加载了
            }
        }

        public UniTask LoadAssetAsync(LoadAssetContext context, LoadAssetPayload payload)
        {
            LoadAsset(ref context, ref payload);
            return UniTask.CompletedTask;
        }
    }
}
