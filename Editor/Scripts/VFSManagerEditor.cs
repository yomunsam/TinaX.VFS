using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TinaX.VFSKit;
using TinaX;
using TinaX.IO;
using TinaX.VFSKit.Const;
using TinaX.VFSKitInternal.Utils;

namespace TinaXEditor.VFSKit
{
    [InitializeOnLoad]
    public static class VFSManagerEditor
    {
        static List<VFSGroup> Groups = new List<VFSGroup>();

        static VFSManagerEditor()
        {
            RefreshManager();
        }

        private static VFSConfigModel mConfig;

        public static void RefreshManager(bool Normalization = false)
        {
            mConfig = XConfig.GetConfig<VFSConfigModel>(VFSConst.ConfigFilePath_Resources);
            if (mConfig == null) return;
            if (Normalization)
            {
                VFSUtil.NormalizationConfig(ref mConfig);
            }

            Groups.Clear();
            if (!mConfig.EnableWebVFS) return;
            foreach(var group_opt in mConfig.Groups)
            {
                Groups.Add(new VFSGroup(group_opt));
            }
            
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <param name="simple">如果true，在检查到“不能被VFS管理”的任何一个条件后就停止查询</param>
        public static void QueryAsset(string path , out AssetsStatusQueryResult result , bool simple = false)
        {
            VFSManagerEditor.QueryAsset(path, mConfig, out result,simple);   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="config"></param>
        /// <param name="result"></param>
        /// <param name="simple">如果true，在检查到“不能被VFS管理”的任何一个条件后就停止查询</param>
        public static void QueryAsset(string path, VFSConfigModel config, out AssetsStatusQueryResult result, bool simple = false)
        {
            result = new AssetsStatusQueryResult();
            result.AssetPath = path;

            if(config == null)
            {
                Debug.LogError("[EDITOR][TinaX.VFS]Can't Query Asset, because config is invalid.");
                return;
            }

            string[] path_items = path.Split('/');
            string ext = XPath.GetExtension(path, true);
            //检查全局规则
            //检查【全局】后缀名
            if (config.GlobalVFS_Ignore_ExtName.Contains(ext))
            {
                //后缀名在忽略列表中
                result.IgnoreByGlobal_IgnoreExtName_List = true;
                if (simple) return;
            }
            result.IgnoreByGlobal_IgnoreExtName_List = false;

            //检查【全局】忽略Path item
            //path_items.Except(config.GlobalVFS_Ignore_Path_Item) //不用LINQ了，需要忽略大小写
            if(config.GlobalVFS_Ignore_Path_Item != null && config.GlobalVFS_Ignore_Path_Item.Length > 0)
            {
                foreach (var item_a in path_items)
                {
                    string item_a_lower = item_a.ToLower();
                    foreach (var item_b_lower in config.GetGlobalVFS_Ignore_Path_Item())
                    {
                        if(item_b_lower == item_a_lower)
                        {
                            result.IgnoreByGlobal_IgnorePathItem_List = true;
                            if (simple)
                                return;
                            else
                                break;
                        }
                    }
                }
            }
            result.IgnoreByGlobal_IgnorePathItem_List = false;

        }





    }

}

