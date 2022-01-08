using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    /// <summary>
    /// 发现资产 异步处理器
    /// </summary>
    public class DiscoverAssetsAsyncHandler : IBuildAssetsAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.DiscoverAssets;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("发现需要构建的资产");
            else
                Debug.Log("Discover assets that need to be built");

            if(context.AssetDiscoverer == null)
            {
                if (context.HansLog)
                    Debug.Log("没有有效的资产发现器，已跳过");
                else
                    Debug.Log("No valid asset discoverer, skipped");
            }
            else
            {
                //发现资产
                if(context.AssetQuerier == null)
                {
                    if (context.HansLog)
                        Debug.LogError("没有有效的资产查询器，已跳过");
                    else
                        Debug.LogError("No valid asset querier, skipped");

                    return Task.CompletedTask;
                }

                context.AssetDiscoverer.DiscoverAssets(context.AssetBundles, context.AssetQuerier);
            }


            return Task.CompletedTask;
        }
    }
}
