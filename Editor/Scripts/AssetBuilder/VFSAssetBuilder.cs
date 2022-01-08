using System.Threading;
using System.Threading.Tasks;
using TinaX.Systems.Pipeline;
using TinaXEditor.Core.Utils.Localization;
using TinaXEditor.VFS.AssetBuilder.Discoverer;
using TinaXEditor.VFS.AssetBuilder.Pipelines;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder
{
    public class VFSAssetBuilder
    {
        private XPipeline<IBuildAssetsAsyncHandler> m_BuildAssetsAsyncPipeline;
        public VFSAssetBuilder()
        {
            m_BuildAssetsAsyncPipeline = BuildAssetsPipelineDefault.CreateAsyncDefault();
        }


        /// <summary>
        /// 异步构建全局资产
        /// </summary>
        /// <returns></returns>
        public Task BuildGlobalAssetsAsync(BuildAssetsArgs args, CancellationToken cancellationToken = default)
        {
            return BuildAssetsAsync(args, new GlobalAssetDiscoverer(), cancellationToken);
        }

        /// <summary>
        /// 异步构建资产
        /// </summary>
        /// <param name="assetDiscoverer">指定资产发现器</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task BuildAssetsAsync(BuildAssetsArgs args, IAssetDiscoverer assetDiscoverer, CancellationToken cancellationToken = default)
        {
            var context = new BuildAssetsContext(args)
            {
                HansLog = EditorLocalizationUtil.IsHans(),
                AssetDiscoverer = assetDiscoverer
            };

            await m_BuildAssetsAsyncPipeline.StartAsync(async handler =>
            {
                await handler.BuildAssetAsync(context, cancellationToken);
                return !context.BreakPipeline;
            });
        }

    }
}
