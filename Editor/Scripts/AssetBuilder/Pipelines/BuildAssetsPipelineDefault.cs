using System;
using TinaX.Systems.Pipeline;
using TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers;

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

            //准备VFS对象
            pipeline.AddLast(new PrepareVFSEditorObjectsAsyncHandler());

            //发现资产
            pipeline.AddLast(new DiscoverAssetsAsyncHandler());

            //构建AssetBundle
            pipeline.AddLast(new BuildAssetBundlesAsyncHandler());

            //复制AssetBundle到Virtual Space
            pipeline.AddLast(new CopyAssetBundleToVirtualSpaceAsyncHandler());

            //保存数据文件
            pipeline.AddLast(new SaveDataFileAsyncHandler());
        }

    }
}
