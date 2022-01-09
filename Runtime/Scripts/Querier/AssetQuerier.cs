using System.IO;
using TinaX.Container;
using TinaX.Systems.Pipeline;
using TinaX.VFS.Packages;
using TinaX.VFS.Packages.Managers;
using TinaX.VFS.Querier.Pipelines;
using TinaX.VFS.SerializableModels.Configurations;

#nullable enable
namespace TinaX.VFS.Querier
{
    /// <summary>
    /// 资产查询器
    /// </summary>
    public class AssetQuerier
    {
        protected XPipeline<IQueryAssetHandler> m_Pipeline;
        private readonly IServiceContainer m_Services;
        private readonly GlobalAssetConfigModel m_GlobalAssetConfig;
        private readonly VFSMainPackage m_MainPackage;
        private readonly ExpansionPackManager m_ExpansionPackManager;

        public AssetQuerier(XPipeline<IQueryAssetHandler> queryPipeline,
            IServiceContainer services,
            GlobalAssetConfigModel globalAssetConfig,
            VFSMainPackage mainPackage,
            ExpansionPackManager expansionPackManager)
        {
            m_Pipeline = queryPipeline;
            this.m_Services = services;
            this.m_GlobalAssetConfig = globalAssetConfig;
            this.m_MainPackage = mainPackage;
            this.m_ExpansionPackManager = expansionPackManager;
        }


        public XPipeline<IQueryAssetHandler> QueryPipeline => m_Pipeline;


        public virtual AssetQueryResult QueryAsset(string assetPath, string variant)
        {
            //上下文
            var queryContext = new QueryAssetContext(m_Services, m_GlobalAssetConfig)
            {
                MainPackage = m_MainPackage,  
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
                handler.QueryAsset(ref queryContext, ref queryresult);
                return !queryContext.BreakPipeline;
            });

            return queryresult;
        }
    }
}
