using System.Linq;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaX.VFS.Packages.Managers;

namespace TinaX.VFS.Querier.Pipelines.Handlers
{
    /// <summary>
    /// 全局忽略规则过滤
    /// </summary>
    public class GlobalIgnoreRulesFilter : IQueryAssetHandler
    {
        public string HandlerName => QueryAssetHandlerNameConsts.FilterByGlobalIgnoreRule;

        public void QueryAsset(ref QueryAssetContext context, ref AssetQueryResult result, ref VFSMainPackage mainPackage, ref ExpansionPackManager expansionPackManager, ref GlobalAssetConfigTpl globalAssetConfig)
        {
            string asset_path_lower = result.AssetPathLower;
            //后缀名过滤
            if(globalAssetConfig.IgnoreExtensions.Any(asset_path_lower.EndsWith))
            {
                //命中后缀名
                result.Valid = false;
                context.Break();
                return;
            }

            //文件夹名过滤
            if (globalAssetConfig.IgnoreFolderName.Any(asset_path_lower.Contains))
            {
                //命中文件夹过滤规则
                result.Valid = false;
                context.Break();
                return;
            }
        }
    }
}
