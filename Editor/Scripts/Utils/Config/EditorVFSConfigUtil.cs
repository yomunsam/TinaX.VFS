using System;
using System.Collections.Generic;
using System.Linq;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.ConfigTpls;
using UnityEditor;
using UnityEngine;
using TinaX;

namespace TinaXEditor.VFS.Utils.Config
{
    public static class EditorVFSConfigUtil
    {
        /// <summary>
        /// 标准化 配置资产 【整个配置资产】
        /// </summary>
        /// <param name="configAsset"></param>
        /// <param name="saveAsset">要不要顺便保存一下</param>
        public static void StandardizedConfigAsset(ref VFSConfigAsset configAsset, bool saveAsset)
        {
            if (configAsset == null)
                throw new ArgumentNullException(nameof(configAsset));

            //全局配置的标准化
            StandardizedIgnoreExtensions(ref configAsset.GlobalIgnoreExtensions); 
            StandardizedIgnoreFolderName(ref configAsset.GlobalIgnoreFolderName);

            //主包
            //主包中的Group
            for(int i = configAsset.MainPackage.Groups.Count -1; i>=0; i--)
            {
                var group = configAsset.MainPackage.Groups[i];
                StandardizedGroup(ref group);
            }

            //保存
            if (saveAsset)
            {
                EditorUtility.SetDirty(configAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 标准化 全局忽略的文件扩展名列表
        /// </summary>
        /// <param name="extensions"></param>
        public static void StandardizedIgnoreExtensions(ref List<string> extensions) //目前好像与Runtime里面的那个一致
        {
            if (extensions.Count < 1)
                return;
            for (var i = extensions.Count - 1; i >= 0; i--)
            {
                //小写和删首尾空格
                extensions[i] = extensions[i].ToLower().Trim();

                //确保前头有点号，比如配置项是"txt",标准化之后是".txt"
                if (!extensions[i].StartsWith("."))
                    extensions[i] = "." + extensions[i];

                //如果配置项就是一个单纯的点，或者是空字符串，就删掉这项
                if (extensions[i] == "." || string.IsNullOrEmpty(extensions[i]) || string.IsNullOrWhiteSpace(extensions[i]))
                    extensions.RemoveAt(i);//因为这儿有Remove，所以这个判断必须放在循环的最后面
            }

            //去重复
            extensions = extensions.Distinct().ToList();
        }

        /// <summary>
        /// 标准化 全局忽略文件夹名
        /// </summary>
        /// <param name="folders"></param>
        public static void StandardizedIgnoreFolderName(ref List<string> folders) //和Runtime的那个不一样
        {
            if (folders.Count < 1) 
                return;

            for (var i = folders.Count - 1; i >= 0; i--)
            {
                //删首尾空格（编辑器下保持原有的大小写，不要改）
                folders[i] = folders[i].Trim();

                //删掉首尾的"/"
                folders[i] = folders[i].TrimStart('/').TrimEnd('/');

                //如果有Assets 或 assets ，删掉；如果配置项是空的也删掉
                string item_lower = folders[i].ToLower();
                if (item_lower == "assets" || string.IsNullOrEmpty(item_lower) || string.IsNullOrWhiteSpace(item_lower))
                    folders.RemoveAt(i); //因为这儿有Remove，所以这个判断必须放在循环的最后面
            }

            //去重复
            folders = folders.Distinct().ToList();
        }

        /// <summary>
        /// 标准化组配置（组之间的查重策略不包含在这儿）
        /// </summary>
        /// <param name="group"></param>
        public static void StandardizedGroup(ref GroupConfigTpl group) //因为组的几个配置耦合度比较高，就都写一起了（因为是编辑器方法，所以不是太追求性能）
        {
            #region GroupName
            group.Name = group.Name.Trim();
            if (string.IsNullOrEmpty(group.Name))
                group.Name = Guid.NewGuid().ToString().GetMD5(true, true);
            #endregion

            //首先AssetPaths的标准化
            for (int i = group.AssetPaths.Count - 1; i >= 0; i--)
            {
                //删首尾空
                string item = group.AssetPaths[i].Trim();
                //删除空项
                if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
                {
                    group.AssetPaths.RemoveAt(i);
                    continue;
                }

                //删掉找不到对应资产的空项
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(item, AssetPathToGUIDOptions.OnlyExistingAssets)))
                {
                    group.AssetPaths.RemoveAt(i);
                    continue;
                }

                group.AssetPaths[i] = item;
            }
            group.AssetPaths = group.AssetPaths.Distinct().ToList(); //去重复

            //顺便来标准化一下IgnoreSubPath的部分规则
            for (int i = group.IgnoreSubPath.Count - 1; i >= 0; i--)
            {
                //删首尾空和尾部的"/"
                string item = group.IgnoreSubPath[i].Trim().TrimEnd('/');
                //删除空项
                if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item) || item == "/")
                {
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }
                //如果是"Assets"则删除
                if (item.ToLower() == "assets")
                {
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }
                group.IgnoreSubPath[i] = item;
            }
            //去重复和排序(排序是为了方便后面做子项之间的嵌套判断)
            group.IgnoreSubPath = group.IgnoreSubPath.Distinct().OrderBy(item => item.Length).ToList();


            //标准化FolderPaths中的部分规则
            for (int i = group.FolderPaths.Count - 1; i >= 0; i--)
            {
                //删首尾空和尾部的"/"
                string item = group.FolderPaths[i].Trim().TrimEnd('/');
                //删除空项
                if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item) || item == "/")
                {
                    group.FolderPaths.RemoveAt(i);
                    continue;
                }
                //如果是"Assets"则删除
                if (item.ToLower() == "assets")
                {
                    group.FolderPaths.RemoveAt(i);
                    continue;
                }
                group.FolderPaths[i] = item;
            }
            //去重复和排序
            group.FolderPaths = group.FolderPaths.Distinct().OrderBy(item => item.Length).ToList();

            //FolderPaths 和 IgnoreSubPath 的对应关系
            var folder_paths_lower_slash_end = group.FolderPaths.Select(item => item.ToLower() + "/").ToList(); //小写，且以"/"结尾的格式（也就是Runtime下预处理的格式）. eg: assets/aaa/bbb/
            var ignore_sub_path_lower_slash_end = group.IgnoreSubPath.Select(item => item.ToLower() + "/").ToList();

            for (int i = ignore_sub_path_lower_slash_end.Count - 1; i >= 0; i--)
            {
                string item = ignore_sub_path_lower_slash_end[i];
                //规则：不可以和FolderPaths中某一项完全一致
                if (folder_paths_lower_slash_end.Contains(item))
                {
#if TINAX_DEV
                    Debug.LogFormat("忽略列表中的 {0} 与FolderPaths中的某一项完全一致，删它.", item);
#endif
                    ignore_sub_path_lower_slash_end.RemoveAt(i);
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }

                //规则：必须是FolderPaths中某一项的子项
                bool is_sub_path_of_an_item_in_the_folderpaths_list = false; //是FolderPaths中某一项的子路径吗？
                for (int j = 0; j < folder_paths_lower_slash_end.Count; j++)
                {
                    string item_j = folder_paths_lower_slash_end[j];
                    if (item_j.Length < item.Length)
                    {
                        if (item.StartsWith(item_j))
                        {
                            is_sub_path_of_an_item_in_the_folderpaths_list = true;
                            break;
                        }
                    }
                }
                if (!is_sub_path_of_an_item_in_the_folderpaths_list)
                {
                    ignore_sub_path_lower_slash_end.RemoveAt(i);
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }
            }

            for (int i = folder_paths_lower_slash_end.Count - 1; i >= 0; i--)
            {
                string item = folder_paths_lower_slash_end[i];

                //规则：自己不允许互相嵌套子项
                if(i > 0)
                {
                    bool i_illegal = false;
                    for (int j = 0; j < i; j++)
                    {
                        string item_j = folder_paths_lower_slash_end[j];
                        //因为之前做过排序了，所以判断后者(i)是不是前者(j)的子项就行
                        if(item_j.Length < item.Length && item.StartsWith(item_j))
                        {
                            //首先能确定i是j的子项了，接下来判断它属不属于一个例外规则：i是IgnoreSubPath中某一项的子路径
                            bool i_is_sub_path_of_k = false;
                            for (int k = 0; k < ignore_sub_path_lower_slash_end.Count; k++)
                            {
                                string item_k = ignore_sub_path_lower_slash_end[k];
                                if (item_k.Length < item.Length && item.StartsWith(item_k))
                                {
                                    i_is_sub_path_of_k = true;
                                    break;
                                }
                            }
                            if (!i_is_sub_path_of_k)
                            {
                                //确实是符合不允许嵌套子项的规则判定，我们干掉它吧
                                i_illegal = true;
                                break;
                            }
                        }
                    }
                    if(i_illegal)
                    {
                        folder_paths_lower_slash_end.RemoveAt(i);
                        group.FolderPaths.RemoveAt(i);
                        continue;
                    }
                }
            }
        }

    }
}
