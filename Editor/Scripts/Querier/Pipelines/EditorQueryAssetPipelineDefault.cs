using System;
using TinaX.Systems.Pipeline;
using EditorGlobalIgnoreRulesFilter = TinaXEditor.VFS.Querier.Pipelines.Handlers.GlobalIgnoreRulesFilter;
using EditorQueryMainPackage = TinaXEditor.VFS.Querier.Pipelines.Handlers.QueryMainPackage;
using EditorHideAssetBundleDirectoryStructure = TinaXEditor.VFS.Querier.Pipelines.Handlers.HideAssetBundleDirectoryStructure;

namespace TinaXEditor.VFS.Querier.Pipelines
{
    public static class EditorQueryAssetPipelineDefault
    {
        public static XPipeline<IEditorQueryAssetHandler> CreateDefault()
        {
            var pipeline = new XPipeline<IEditorQueryAssetHandler>();
            AppendDefaultPipeline(ref pipeline);
            return pipeline;
        }

        /// <summary>
        /// 追加默认的Pipeline
        /// 如果给的是空的Pipeline，就相当于设置Pipeline
        /// </summary>
        /// <param name="pipeline"></param>
        public static void AppendDefaultPipeline(ref XPipeline<IEditorQueryAssetHandler> pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            //全局忽略规则的筛选
            pipeline.AddLast(new EditorGlobalIgnoreRulesFilter());

            //在主包中查询资产
            pipeline.AddLast(new EditorQueryMainPackage());
            //Todo:在扩展包中查询资产

            //路径处理：隐藏AssetBundle中的目录结构
            pipeline.AddLast(new EditorHideAssetBundleDirectoryStructure());
        }
    }
}
