using Cysharp.Threading.Tasks;
using TinaX.Exceptions;
using UnityEngine;
using Debug = UnityEngine.Debug;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset.Handlers
{
    public class QueryAssetHandler : ILoadAssetHandler, ILoadAssetAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.QueryAsset;

        public void LoadAsset(ref LoadAssetContext context, ref LoadAssetPayload payload)
        {
#if TINAX_DEV
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            if (payload.QueryResult != null)
                return;
            payload.QueryResult = context.AssetQuerier.QueryAsset(payload.LoadPath, payload.Variant);

#if TINAX_DEV
            sw.Stop();
            Debug.LogFormat("查询资产用时：{0}", sw.Elapsed.TotalSeconds.ToString("N3"));
#endif

            if (!payload.QueryResult.Value.Valid) //查询无效的话则异常
            {
                throw new XException(context.HansLog ? $"被加载的Asset路径无效，它未被VFS管理：{payload.LoadPath}" : $"The loaded Asset path is invalid, it is not managed by VFS: {payload.LoadPath}");
            }
        }

        public UniTask LoadAssetAsync(LoadAssetContext context, LoadAssetPayload payload)
        {
            LoadAsset(ref context, ref payload);
            return UniTask.CompletedTask;
        }
    }

}
