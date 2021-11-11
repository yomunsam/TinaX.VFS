using TinaX.VFS.ConfigTpls;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;

namespace TinaXEditor.VFS.Querier
{
    /// <summary>
    /// 编辑器资产查询器接口
    /// </summary>
    public interface IEditorAssetQuerier
    {
        EditorAssetQueryResult QueryAsset(string assetPath, EditorMainPackage mainPackage, EditorExpansionPackManager expansionPackManager, GlobalAssetConfigTpl globalAssetConfigTpl);
    }
}
