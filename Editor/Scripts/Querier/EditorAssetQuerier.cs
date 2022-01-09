using System.IO;
using TinaX.Systems.Pipeline;
using TinaX.VFS.SerializableModels.Configurations;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;
using TinaXEditor.VFS.Querier.Pipelines;

#nullable enable
namespace TinaXEditor.VFS.Querier
{
    /// <summary>
    /// 编辑器下的资产查询器
    /// </summary>
    public class EditorAssetQuerier : IEditorAssetQuerier
    {
        protected readonly XPipeline<IEditorQueryAssetHandler> m_Pipleline;
        private readonly EditorMainPackage m_MainPackage;
        private readonly EditorExpansionPackManager m_ExpansionPackManager;
        private readonly GlobalAssetConfigModel m_GlobalAssetConfig;

        public EditorAssetQuerier(XPipeline<IEditorQueryAssetHandler> queryPipeline, 
            EditorMainPackage mainPackage, 
            EditorExpansionPackManager expansionPackManager, 
            GlobalAssetConfigModel globalAssetConfig)
        {
            this.m_Pipleline = queryPipeline;
            this.m_MainPackage = mainPackage;
            this.m_ExpansionPackManager = expansionPackManager;
            this.m_GlobalAssetConfig = globalAssetConfig;
        }


        public virtual EditorAssetQueryResult QueryAsset(string assetPath)
        {
            //上下文
            var queryContext = new EditorQueryAssetContext();
            var queryResult = new EditorAssetQueryResult
            {
                AssetPath = assetPath,
                AssetPathLower = assetPath.ToLower(),
                AssetExtension = Path.GetExtension(assetPath),
            };
            queryResult.VirtualAssetPath = queryResult.AssetPath;
            queryResult.VirtualAssetPathLower = queryResult.VirtualAssetPathLower;

            m_Pipleline.Start(handler =>
            {
                handler.QueryAsset(ref queryContext, ref queryResult, in m_MainPackage, in m_ExpansionPackManager, in m_GlobalAssetConfig);
                return !queryContext.BreakPipeline;
            });
            if (string.IsNullOrEmpty(queryResult.VariantName) && !queryResult.IsVariant)
                queryResult.VariantName = m_GlobalAssetConfig.DefaultAssetBundleVariant.Trim().TrimStart('.');

            return queryResult;
        }

    }
}
