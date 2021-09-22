using TinaX.Systems.Pipeline;
using TinaX.VFS.Pipelines.LoadVFSConfigAsset;

namespace TinaX.VFS.Extensions
{
    /// <summary>
    /// pipeline相关扩展
    /// </summary>
    public static class PipelineExtensions
    {

        public static XPipeline<ILoadVFSConfigAssetHandler> Use(this XPipeline<ILoadVFSConfigAssetHandler> pipeline, string name, GeneralLoadVFSConfigAssetHandler.LoadVFSConfigAssetAsyncDelegate handlerFunc)
        {
            pipeline.AddLast(new GeneralLoadVFSConfigAssetHandler(name, handlerFunc));
            return pipeline;
        }
    }
}
