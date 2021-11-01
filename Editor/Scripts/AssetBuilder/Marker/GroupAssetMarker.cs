using System.Collections.Generic;
using System.Linq;
using TinaXEditor.VFS.AssetBuilder.Structs;
using TinaXEditor.VFS.Groups;
using TinaXEditor.VFS.Managers;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

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
            if (EditorVFSManager.AssetQuerier == null)
                EditorVFSManager.InitializeAssetQuerier();

            //整理所有被我们Group管理的Asset们
            List<AssetPathAndGuid> groupAssets;
            Group.GetAllManagedAssets(out groupAssets);

            Debug.LogFormat("Count:{0}", groupAssets.Count);

            foreach(var item in groupAssets)
            {
                var query_result = EditorVFSManager.QueryAsset(item.AssetPath);

                if (!query_result.Valid)
                    return;
                Debug.LogFormat("---- {0} v:{1}", query_result.AssetBundleName, query_result.VariantName);

                var assetImporter = AssetImporter.GetAtPath(item.AssetPath);
                if (assetImporter.assetBundleName != query_result.AssetBundleName || assetImporter.assetBundleVariant != query_result.VariantName)
                {
                    assetImporter.SetAssetBundleNameAndVariant(query_result.AssetBundleName, query_result.VariantName);
                }
            }
        }




    }
}
