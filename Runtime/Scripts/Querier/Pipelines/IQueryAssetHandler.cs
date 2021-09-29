using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaX.VFS.Packages.Managers;

namespace TinaX.VFS.Querier.Pipelines
{
    /// <summary>
    /// 管线 查询资产处理者 接口
    /// </summary>
    public interface IQueryAssetHandler
    {
        string HandlerName { get; }

        void QueryAsset(ref QueryAssetContext context, ref AssetQueryResult result, ref VFSMainPackage mainPackage, ref ExpansionPackManager expansionPackManager, ref GlobalAssetConfigTpl globalAssetConfig);
    }
}
