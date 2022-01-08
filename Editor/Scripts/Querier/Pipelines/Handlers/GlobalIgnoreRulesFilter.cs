using System.Linq;
using TinaX.VFS.ConfigTpls;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;

namespace TinaXEditor.VFS.Querier.Pipelines.Handlers
{
    /// <summary>
    /// 全局忽略规则过滤
    /// </summary>
    public class GlobalIgnoreRulesFilter : IEditorQueryAssetHandler
    {
        public string HandlerName => EditorQueryAssetHandlerNameConsts.FilterByGlobalIgnoreRule;

        public void QueryAsset(ref EditorQueryAssetContext context, ref EditorAssetQueryResult result, in EditorMainPackage mainPackage, in EditorExpansionPackManager expansionPackManager, in GlobalAssetConfigTpl globalAssetConfig)
        {
            //后缀名过滤
            if (globalAssetConfig.IgnoreExtensions.Any(result.AssetPathLower.EndsWith))
            {
                //命中后缀名
                result.Valid = false;
                context.Break();
                return;
            }

            //文件夹过滤
            if(globalAssetConfig.IgnoreFolderNames.Any(result.AssetPathLower.Contains))
            {
                result.Valid = false;
                context.Break();
                return;
            }
        }
    }
}
