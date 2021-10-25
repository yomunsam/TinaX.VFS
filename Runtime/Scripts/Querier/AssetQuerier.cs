using System.IO;
using TinaX.Container;
using TinaX.Systems.Pipeline;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaX.VFS.Packages.Managers;
using TinaX.VFS.Querier.Pipelines;

namespace TinaX.VFS.Querier
{
    /// <summary>
    /// 资产查询器
    /// </summary>
    public class AssetQuerier
    {
        protected XPipeline<IQueryAssetHandler> m_Pipeline;

        public AssetQuerier(QueryAssetPipelineBuilder pipelineBuilder)
        {
            m_Pipeline = pipelineBuilder.Pipeline;
        }

        public AssetQuerier(XPipeline<IQueryAssetHandler> queryPipeline)
        {
            m_Pipeline = queryPipeline;
        }


        public XPipeline<IQueryAssetHandler> QueryPipeline => m_Pipeline;


        public virtual AssetQueryResult QueryAsset(string assetPath, string variant, IServiceContainer services, VFSMainPackage mainPackage, ExpansionPackManager expansionPackManager, GlobalAssetConfigTpl globalAssetConfigTpl)
        {
            //上下文
            var queryContext = new QueryAssetContext
            {
                Services = services
            };
            var queryresult = new AssetQueryResult
            {
                VirtualAssetPath = assetPath,
                VirtualAssetPathLower = assetPath.ToLower(),
                AssetExtension = Path.GetExtension(assetPath),
                VariantName = variant
            };

            m_Pipeline.Start(handler =>
            {
                handler.QueryAsset(ref queryContext, ref queryresult, ref mainPackage, ref expansionPackManager, ref globalAssetConfigTpl);
                return !queryContext.BreakPipeline;
            });

            return queryresult;
        }
    }
}
