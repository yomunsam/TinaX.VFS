using System;
using TinaX.Systems.Pipeline;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines
{
    public static class BuildAssetsPipelineDefault
    {
        public static XPipeline<IBuildAssetsAsyncHandler> CreateAsyncDefault()
        {
            var pipeline = new XPipeline<IBuildAssetsAsyncHandler>();
            AppendDefaultAsyncPipeline(ref pipeline);
            return pipeline;
        }


        public static void AppendDefaultAsyncPipeline(ref XPipeline<IBuildAssetsAsyncHandler> pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

        }

    }
}
