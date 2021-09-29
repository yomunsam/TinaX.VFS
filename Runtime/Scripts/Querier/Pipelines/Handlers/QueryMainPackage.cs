using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaX.VFS.Packages.Managers;

namespace TinaX.VFS.Querier.Pipelines.Handlers
{
    public class QueryMainPackage : IQueryAssetHandler
    {
        public string HandlerName => QueryAssetHandlerNameConsts.QueryFromMainPackage;

        public void QueryAsset(ref QueryAssetContext context, ref AssetQueryResult result, ref VFSMainPackage mainPackage, ref ExpansionPackManager expansionPackManager, ref GlobalAssetConfigTpl globalAssetConfig)
        {
            if (result.ManagedPackage != null)
                return; //资产的归属已经找到了，本流程就不需要执行了。

            if (mainPackage.TryQueryAsset(ref result))
            {
                //查到了
                result.ManagedByMainPack = true;
                result.Valid = true;
                //这时候查询流程其实已经搞完了，但我们还是继续往后走Pipeline,以便进行一些路径啥的后期处理
            }
        }
    }
}
