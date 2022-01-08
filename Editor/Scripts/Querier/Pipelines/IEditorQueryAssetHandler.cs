using TinaX.VFS.ConfigTpls;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;

namespace TinaXEditor.VFS.Querier.Pipelines
{
    /// <summary>
    /// 编辑器下的 管线 查询资产处理者 接口
    /// </summary>
    public interface IEditorQueryAssetHandler
    {
        string HandlerName { get; }

        void QueryAsset(ref EditorQueryAssetContext context, 
            ref EditorAssetQueryResult result, 
            in EditorMainPackage mainPackage, 
            in EditorExpansionPackManager expansionPackManager, 
            in GlobalAssetConfigTpl globalAssetConfig);
    }
}
