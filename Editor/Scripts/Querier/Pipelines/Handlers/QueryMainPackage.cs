using TinaX.VFS.ConfigTpls;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;

namespace TinaXEditor.VFS.Querier.Pipelines.Handlers
{
    public class QueryMainPackage : IEditorQueryAssetHandler
    {
        public string HandlerName => EditorQueryAssetHandlerNameConsts.QueryFromMainPackage;

        public void QueryAsset(ref EditorQueryAssetContext context, ref EditorAssetQueryResult result, in EditorMainPackage mainPackage, in EditorExpansionPackManager expansionPackManager, in GlobalAssetConfigTpl globalAssetConfig)
        {
            if (!context.QueryMainPack)
                return; //本次查询不在主包中查询。

            if (result.ManagedGroup != null)
                return; //资产的归属已经找到了，本流程就不需要执行了。

            if(mainPackage.TryQueryAsset(ref result))
            {
                //查到了
                result.ManagedByMainPack = true;
                result.Valid = true;
                //这时候查询流程其实已经搞完了，但我们还是继续往后走Pipeline,以便进行一些路径啥的后期处理
            }
        }
    }
}
