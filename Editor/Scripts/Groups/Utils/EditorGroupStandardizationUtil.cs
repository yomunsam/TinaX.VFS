using System;
using System.Linq;
using TinaX;
using TinaX.VFS.ConfigAssets.Configurations;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFS.Groups.Utils
{
    /// <summary>
    /// 编辑器的 VFS 资产组 标准化工具
    /// </summary>
    public static class EditorGroupStandardizationUtil
    {
        /// <summary>
        /// 对组的标准化
        /// </summary>
        /// <param name="group"></param>
        public static void StandardizeGroup(GroupConfig group)
        {
            if(group == null)
                throw new ArgumentNullException(nameof(group));
            //由于组里的配置耦合度比较高，所以我们就暂时先都写在一起了，以后如果实在不美观的话再拆分（嘛弄不好哪天就重构了（flag+1））

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
                if (string.IsNullOrEmpty(item))
                {
                    group.AssetPaths.RemoveAt(i);
                    continue;
                }

                //删掉找不到对应资产的空项
#if UNITY_2021_1_OR_NEWER
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(item, AssetPathToGUIDOptions.OnlyExistingAssets)))
#else
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(item)))
#endif
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
                if (i > 0)
                {
                    bool i_illegal = false;
                    for (int j = 0; j < i; j++)
                    {
                        string item_j = folder_paths_lower_slash_end[j];
                        //因为之前做过排序了，所以判断后者(i)是不是前者(j)的子项就行
                        if (item_j.Length < item.Length && item.StartsWith(item_j))
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
                    if (i_illegal)
                    {
                        folder_paths_lower_slash_end.RemoveAt(i);
                        group.FolderPaths.RemoveAt(i);
                        continue;
                    }
                }
            }

            //FolderSpecialBuildRules 目录特殊构建规则
            for (int i = group.FolderSpecialBuildRules.Count - 1; i >= 0; i--)
            {
                var rule = group.FolderSpecialBuildRules[i];
                //删首尾空 和尾部可能出现的"/"
                rule.Path = rule.Path.Trim().TrimEnd('/');
                
                //删除空项
                if (rule.Path.IsNullOrEmpty())
                {
                    group.FolderSpecialBuildRules.RemoveAt(i);
                    continue;
                }
                group.FolderSpecialBuildRules[i] = rule;
            }
            //排序
            group.FolderSpecialBuildRules = group.FolderSpecialBuildRules.OrderBy(rule => rule.Path.Length).ToList();

            //AssetVariants 资产变体列表
            for (int i = group.AssetVariants.Count - 1; i >= 0; i--)
            {
                var rule = group.AssetVariants[i];
                //删首尾空
                rule.SourceAssetPath = rule.SourceAssetPath.Trim();
                //删除空项
                if (string.IsNullOrEmpty(rule.SourceAssetPath))
                {
                    group.AssetVariants.RemoveAt(i);
                    continue;
                }

                //处理Variants列表
                for (int j = rule.Variants.Count - 1; j >= 0; j--)
                {
                    var item = rule.Variants[j];
                    //删首尾空
                    item.AssetPath = item.AssetPath.ToLower(); //Editor下不小写
                    item.Variant = item.Variant.Trim().ToLower(); //即使在Editor下也小写

                    //删除空项
                    if (item.Variant.IsNullOrEmpty() || item.AssetPath.IsNullOrEmpty())
                    {
                        rule.Variants.RemoveAt(j);
                        continue;
                    }

                    rule.Variants[j] = item;
                }
                //排序Variants列表
                rule.Variants = rule.Variants.OrderBy(v => v.AssetPath).ToList();

                group.AssetVariants[i] = rule;
            }

            //FolderVariants 资产变体（文件夹）
            for (int i = group.FolderVariants.Count - 1; i >= 0; i--)
            {
                var rule = group.FolderVariants[i];
                //删首尾空
                rule.SourceFolderPath = rule.SourceFolderPath.Trim().TrimEnd('/');

                //删除空项
                if (string.IsNullOrEmpty(rule.SourceFolderPath))
                {
                    group.AssetVariants.RemoveAt(i);
                    continue;
                }

                //处理Variants列表
                for (int j = rule.Variants.Count - 1; j >= 0; j--)
                {
                    var item = rule.Variants[j];
                    //删首尾空 和 小写
                    item.FolderPath = item.FolderPath.Trim().TrimEnd('/');
                    item.Variant = item.Variant.Trim().ToLower();

                    //删除空项
                    if (item.Variant.IsNullOrEmpty() || item.FolderPath.IsNullOrEmpty())
                    {
                        rule.Variants.RemoveAt(j);
                        continue;
                    }

                    rule.Variants[j] = item;
                }
                //排序Variants列表
                rule.Variants = rule.Variants.OrderBy(v => v.FolderPath).ToList();

                group.FolderVariants[i] = rule;
            }
        }
    }
}
