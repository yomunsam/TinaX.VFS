using System;
using System.Collections.Generic;
using System.Linq;
using TinaX.VFS.ConfigAssets;
using TinaXEditor.VFS.Groups.Utils;
using UnityEditor;

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
            StandardizedIgnoreExtensions(ref configAsset.GlobalAssetConfig.IgnoreExtensions); 
            StandardizedIgnoreFolderName(ref configAsset.GlobalAssetConfig.IgnoreFolderName);

            //主包
            //主包中的Group
            for(int i = configAsset.MainPackage.Groups.Count -1; i>=0; i--)
            {
                var group = configAsset.MainPackage.Groups[i];
                EditorGroupStandardizationUtil.StandardizeGroup(group);
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

        

    }
}
