using System.IO;
using TinaX.Systems.Pipeline;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Querier;
using TinaX.VFS.Querier.Pipelines;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;
using TinaXEditor.VFS.Querier.Pipelines;

namespace TinaXEditor.VFS.Querier
{
    /// <summary>
    /// 编辑器下的资产查询器
    /// </summary>
    public class EditorAssetQuerier
    {
        protected readonly XPipeline<IEditorQueryAssetHandler> m_Pipleline;

        public EditorAssetQuerier(XPipeline<IEditorQueryAssetHandler> queryPipeline)
        {
            this.m_Pipleline = queryPipeline;
        }


        public virtual EditorAssetQueryResult QueryAsset(string assetPath, EditorMainPackage mainPackage, EditorExpansionPackManager expansionPackManager, GlobalAssetConfigTpl globalAssetConfigTpl)
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
                handler.QueryAsset(ref queryContext, ref queryResult, ref mainPackage, ref expansionPackManager, ref globalAssetConfigTpl);
                return !queryContext.BreakPipeline;
            });
            if (string.IsNullOrEmpty(queryResult.VariantName) && !queryResult.IsVariant)
                queryResult.VariantName = globalAssetConfigTpl.DefaultAssetBundleVariant;

            return queryResult;
        }

    }
}
