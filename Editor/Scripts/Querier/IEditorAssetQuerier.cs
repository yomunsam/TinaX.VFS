namespace TinaXEditor.VFS.Querier
{
    /// <summary>
    /// 编辑器资产查询器接口
    /// </summary>
    public interface IEditorAssetQuerier
    {
        EditorAssetQueryResult QueryAsset(string assetPath);
    }
}
