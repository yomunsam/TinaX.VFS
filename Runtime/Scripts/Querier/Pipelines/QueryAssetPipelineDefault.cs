using System;
using TinaX.Systems.Pipeline;
using TinaX.VFS.Querier.Pipelines.Handlers;

namespace TinaX.VFS.Querier.Pipelines
{
    /// <summary>
    /// 查询资产管线构建器
    /// </summary>
    public static class QueryAssetPipelineDefault
    {

        public static XPipeline<IQueryAssetHandler> CreateDefault()
        {
            var pipeline = new XPipeline<IQueryAssetHandler>();
            AppendDefaultPipeline(ref pipeline);
            return pipeline;
        }

        /// <summary>
        /// 追加默认的Pipeline
        /// 如果给的是空的Pipeline，就相当于设置Pipeline
        /// </summary>
        /// <param name="pipeline"></param>
        public static void AppendDefaultPipeline(ref XPipeline<IQueryAssetHandler> pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            //全局忽略规则的筛选
            pipeline.AddLast(new GlobalIgnoreRulesFilter());
            //在主包中查询资产
            pipeline.AddLast(new QueryMainPackage());
            //Todo:在扩展包中查询资产

            //路径处理：隐藏AssetBundle中的目录结构
            pipeline.AddLast(new HideAssetBundleDirectoryStructure());
        }
    }
}
