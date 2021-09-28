using System;
using System.Collections.Generic;
using TinaXEditor.VFS.AssetBuilder.Structs;
using TinaXEditor.VFS.Groups;
using UnityEditor;
using UnityEngine;
using TinaX;
using System.Linq;

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
            var config = Group.Config;
            //整理所有被我们Group管理的Asset们
            List<string> groupAssets = new List<string>();

            var startTime = DateTime.Now;
            //组配置的AssetPaths
            foreach(var item in config.AssetPaths)
            {
                var guid = AssetDatabase.AssetPathToGUID(item);
                if(!string.IsNullOrEmpty(guid))
                {
                    groupAssets.Add(item);
                }
            }
#if TINAX_DEV
            Debug.LogFormat("AssetPaths中共有{0}个有效资产", groupAssets.Count);
            var useTime = DateTime.Now - startTime;
            Debug.LogFormat("耗时：{0}", useTime.TotalSeconds.ToString("N3"));
#endif
            //然后看看FolderPaths里
            var guids = AssetDatabase.FindAssets("", config.FolderPaths.ToArray());
            foreach (var guid in guids)
            {
                string path =AssetDatabase.GUIDToAssetPath(guid);
            }

#if TINAX_DEV
            Debug.LogFormat("当前已有资产总数:{0}", groupAssets.Count);
            useTime = DateTime.Now - startTime;
            Debug.LogFormat("耗时：{0}", useTime.TotalSeconds.ToString("N3"));
#endif
        }




    }
}
