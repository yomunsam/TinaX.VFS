using System;
using TinaX.Systems.Pipeline;
using TinaX.VFS.Pipelines.LoadAsset.Handlers;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset
{
    public static class LoadAssetPipelineDefault
    {
        public static XPipeline<ILoadAssetAsyncHandler> CreateAsyncDefault()
        {
            var pipeline = new XPipeline<ILoadAssetAsyncHandler>();
            AppendDefaultAsyncPipeline(ref pipeline);
            return pipeline;
        }


        public static void AppendDefaultAsyncPipeline(ref XPipeline<ILoadAssetAsyncHandler> pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            //查询资产
            pipeline.AddLast(new QueryAssetHandler());

            //检查已加载（对象池）
            pipeline.AddLast(new CheckLoadedAssetHandler());
        }


        public static XPipeline<ILoadAssetHandler> CreateDefault()
        {
            var pipeline = new XPipeline<ILoadAssetHandler>();
            AppendDefaultPipeline(ref pipeline);
            return pipeline;
        }


        public static void AppendDefaultPipeline(ref XPipeline<ILoadAssetHandler> pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            //查询资产
            pipeline.AddLast(new QueryAssetHandler());

            //检查已加载（对象池）
            pipeline.AddLast(new CheckLoadedAssetHandler());
        }
    }
}
