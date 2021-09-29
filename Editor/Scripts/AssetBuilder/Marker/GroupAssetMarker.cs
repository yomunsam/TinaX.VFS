using System.Collections.Generic;
using TinaXEditor.VFS.AssetBuilder.Structs;
using TinaXEditor.VFS.Groups;

namespace TinaXEditor.VFS.AssetBuilder.Marker
{
    /// <summary>
    /// VFS 资产组 AssetBundle 标记器
    /// 用于根据配置来设置资产的AssetBundle名字和变体信息
    /// </summary>
    public class GroupAssetMarker
    {
        public GroupAssetMarker() { }
        public GroupAssetMarker(EditorVFSGroup group)
        {
            Group = group;
        }

        public EditorVFSGroup Group { get; }

        public void Mark()
        {
            //整理所有被我们Group管理的Asset们
            List<AssetPathAndGuid> groupAssets;
            Group.GetAllManagedAssets(out groupAssets);

        }




    }
}
