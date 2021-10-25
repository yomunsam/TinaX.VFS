using System.Linq;
using TinaX.VFS.ConfigTpls;
using UnityEngine;

namespace TinaX.VFS.Groups.Utils
{
    /// <summary>
    /// VFS 资产组 标准化工具
    /// </summary>
    public static class GroupStandardizationUtil
    {
        /// <summary>
        /// 对组的标准化
        /// </summary>
        /// <param name="group"></param>
        public static void StandardizeGroup(GroupConfigTpl group)
        {
            // 标准化 AssetPaths
            for (int i = group.AssetPaths.Count - 1; i >= 0; i--)
            {
                //删首尾空
                string item = group.AssetPaths[i].Trim().ToLower();
                //删除空项
                if (string.IsNullOrEmpty(item))
                {
                    group.AssetPaths.RemoveAt(i);
                    continue;
                }
                group.AssetPaths[i] = item;
            }
            group.AssetPaths = group.AssetPaths.Distinct().ToList(); //去重复


            // 标准化 IgnoreSubPath 的部分规则
            for (int i = group.IgnoreSubPath.Count - 1; i >= 0; i--)
            {
                //删首尾空+小写
                string item = group.IgnoreSubPath[i].Trim().ToLower();
                if (!item.EndsWith("/")) //末尾加斜杠
                    item += "/";

                //删除空项
                if (string.IsNullOrEmpty(item) || item == "/")
                {
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }
                //如果是"Assets"则删除
                if (item.ToLower() == "assets/")
                {
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }
                group.IgnoreSubPath[i] = item;
            }
            //去重复和排序(排序是为了方便后面做子项之间的嵌套判断)（排序是把文本长度比较长的路径放在后面，方便倒序遍历的时候用）
            group.IgnoreSubPath = group.IgnoreSubPath.Distinct().OrderBy(item => item.Length).ToList();


            //标准化 FolderPaths 中的部分规则
            for (int i = group.FolderPaths.Count - 1; i >= 0; i--)
            {
                //删首尾空+小写
                string item = group.FolderPaths[i].Trim().ToLower();
                if (!item.EndsWith("/")) //末尾加斜杠
                    item += "/";

                //删除空项
                if (string.IsNullOrEmpty(item) || item == "/")
                {
                    group.FolderPaths.RemoveAt(i);
                    continue;
                }
                //如果是"Assets"则删除
                if (item.ToLower() == "assets/")
                {
                    group.FolderPaths.RemoveAt(i);
                    continue;
                }
                group.FolderPaths[i] = item;
            }
            //去重复和排序
            group.FolderPaths = group.FolderPaths.Distinct().OrderBy(item => item.Length).ToList();



            //FolderPaths 和 IgnoreSubPath 的对应关系
            for (int i = group.IgnoreSubPath.Count - 1; i >= 0; i--)
            {
                string item = group.IgnoreSubPath[i];
                //规则：不可以和FolderPaths中某一项完全一致
                if (group.FolderPaths.Contains(item))
                {
#if TINAX_DEV
                    Debug.LogFormat("忽略列表中的 {0} 与FolderPaths中的某一项完全一致，删它.", item);
#endif
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }

                //规则：必须是FolderPaths中某一项的子项
                bool is_sub_path_of_an_item_in_the_folderpaths_list = false; //是FolderPaths中某一项的子路径吗？
                for (int j = 0; j < group.FolderPaths.Count; j++)
                {
                    string item_j = group.FolderPaths[j];
                    if (item_j.Length < item.Length)
                    {
                        if (item.StartsWith(item_j))
                        {
                            is_sub_path_of_an_item_in_the_folderpaths_list = true;
                            break;
                        }
                    }
                    else
                        break; //因为前面有排序了，所以如果有一个j的长度>=i的话，后面就都大于了，就不用继续循环了
                }
                if (!is_sub_path_of_an_item_in_the_folderpaths_list)
                {
                    group.IgnoreSubPath.RemoveAt(i);
                    continue;
                }
            }

            for (int i = group.FolderPaths.Count - 1; i >= 0; i--)
            {
                string item = group.FolderPaths[i];

                //规则：自己不允许互相嵌套子项
                if (i > 0)
                {
                    bool i_illegal = false;
                    for (int j = 0; j < i; j++)
                    {
                        string item_j = group.FolderPaths[j];
                        //因为之前做过排序了，所以判断后者(i)是不是前者(j)的子项就行
                        if (item_j.Length < item.Length)
                        {
                            if (item.StartsWith(item_j))
                            {
                                //首先能确定i是j的子项了，接下来判断它属不属于一个例外规则：i是IgnoreSubPath中某一项的子路径
                                bool i_is_sub_path_of_k = false;
                                for (int k = 0; k < group.IgnoreSubPath.Count; k++)
                                {
                                    string item_k = group.IgnoreSubPath[k];
                                    if (item_k.Length < item.Length)
                                    {
                                        if (item.StartsWith(item_k))
                                        {
                                            i_is_sub_path_of_k = true;
                                            break;
                                        }
                                    }
                                    else
                                        break;
                                }
                                if (!i_is_sub_path_of_k)
                                {
                                    //确实是符合不允许嵌套子项的规则判定，我们干掉它吧
                                    i_illegal = true;
                                    break;
                                }
                            }
                        }
                        else
                            break; //一样的道理，因为我们有按照长度排序过，一旦i不比j长，就不用再循环了
                    }
                    if (i_illegal)
                    {
                        group.FolderPaths.RemoveAt(i);
                        continue;
                    }
                }
            }

            //FolderSpecialBuildRules 目录特殊构建规则
            for(int i = group.FolderSpecialBuildRules.Count -1; i >=0; i--)
            {
                var rule = group.FolderSpecialBuildRules[i];
                //删首尾空 和 小写化
                rule.Path = rule.Path.Trim().ToLower();
                if (!rule.Path.EndsWith("/")) //末尾加斜杠
                    rule.Path += "/";
                //删除空项
                if(rule.Path.IsNullOrEmpty() || rule.Path == "/")
                {
                    group.FolderSpecialBuildRules.RemoveAt(i);
                    continue;
                }
                group.FolderSpecialBuildRules[i] = rule;
            }
            //排序
            group.FolderSpecialBuildRules = group.FolderSpecialBuildRules.OrderBy(rule => rule.Path.Length).ToList();

            //AssetVariants 资产变体列表
            for (int i = group.AssetVariants.Count -1; i >=0; i--)
            {
                var rule = group.AssetVariants[i];
                //删首尾空
                rule.SourceAssetPath = rule.SourceAssetPath.Trim();
                //删除空项
                if(string.IsNullOrEmpty(rule.SourceAssetPath))
                {
                    group.AssetVariants.RemoveAt(i);
                    continue;
                }

                //处理Variants列表
                for(int j = rule.Variants.Count -1; j >=0; j--)
                {
                    var item = rule.Variants[j];
                    //删首尾空 和 小写
                    item.AssetPath = item.AssetPath.ToLower().Trim();
                    item.Variant = item.Variant.Trim().ToLower();

                    //删除空项
                    if(item.Variant.IsNullOrEmpty() || item.AssetPath.IsNullOrEmpty())
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
            for(int i = group.FolderVariants.Count -1; i >=0; i--)
            {
                var rule = group.FolderVariants[i];
                //删首尾空
                rule.SourceFolderPath = rule.SourceFolderPath.Trim();
                if (!rule.SourceFolderPath.EndsWith("/"))
                    rule.SourceFolderPath += "/"; //结尾加斜杠

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
                    item.FolderPath = item.FolderPath.ToLower().Trim();
                    item.Variant = item.Variant.Trim().ToLower();
                    if (!item.FolderPath.EndsWith("/")) //结尾加斜杠
                        item.FolderPath += "/";

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
