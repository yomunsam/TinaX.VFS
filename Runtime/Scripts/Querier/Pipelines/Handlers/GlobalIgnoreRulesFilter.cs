using System.Linq;

namespace TinaX.VFS.Querier.Pipelines.Handlers
{
    /// <summary>
    /// 全局忽略规则过滤
    /// </summary>
    public class GlobalIgnoreRulesFilter : IQueryAssetHandler
    {
        public string HandlerName => QueryAssetHandlerNameConsts.FilterByGlobalIgnoreRule;

        public void QueryAsset(ref QueryAssetContext context, ref AssetQueryResult result)
        {
            string asset_path_lower = result.VirtualAssetPathLower;
            //后缀名过滤
            if(context.GlobalAssetConfig.IgnoreExtensions.Any(asset_path_lower.EndsWith))
            {
                //命中后缀名
                result.Valid = false;
                context.Break();
                return;
            }

            //文件夹名过滤
            if (context.GlobalAssetConfig.IgnoreFolderNames.Any(asset_path_lower.Contains))
            {
                //命中文件夹过滤规则
                result.Valid = false;
                context.Break();
                return;
            }
        }
    }
}
