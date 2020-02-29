using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using TinaX;
using System.IO;
using TinaX.IO;
using TinaXEditor.VFSKit.Const;
using TinaX.VFSKit.Const;

namespace TinaXEditor.VFSKit.Utils
{
    using Object = UnityEngine.Object;

    public static class VFSEditorUtil
    {
        internal static bool GetPathAndGUIDFromTarget(Object t, out string path, ref string guid, out Type mainAssetType)
        {
            mainAssetType = null;
            path = AssetDatabase.GetAssetOrScenePath(t);
            
            guid = AssetDatabase.AssetPathToGUID(path);
            if (guid.IsNullOrEmpty())
                return false;
            mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (mainAssetType != t.GetType() && !typeof(AssetImporter).IsAssignableFrom(t.GetType()))
                return false;
            return true;
        }

        internal static void InitVFSFoldersInStreamingAssets()
        {
            string project_root_path = Directory.GetCurrentDirectory();
            //tinax
            XDirectory.DeleteIfExists(Path.Combine(project_root_path, VFSConst.VFS_STREAMINGASSETS_PATH));
            XDirectory.CreateIfNotExists(Path.Combine(project_root_path, VFSConst.VFS_STREAMINGASSETS_PATH));
            //XDirectory.CreateIfNotExists(Path.Combine(project_root_path, VFSConst.VFS_STREAMINGASSETS_PATH,VFSConst.VFS_FOLDER_MAIN));
            //XDirectory.CreateIfNotExists(Path.Combine(project_root_path, VFSConst.VFS_STREAMINGASSETS_PATH,VFSConst.VFS_FOLDER_DATA));
            //XDirectory.CreateIfNotExists(Path.Combine(project_root_path, VFSConst.VFS_STREAMINGASSETS_PATH,VFSConst.VFS_FOLDER_EXTENSION));
        }

        public static void RemoveAllAssetbundleSigns(bool showEditorGUI = true)
        {
            var ab_names = AssetDatabase.GetAllAssetBundleNames();

            int counter = 0;
            int counter_t = 0;
            int length = ab_names.Length;

            foreach(var name in ab_names)
            {
                AssetDatabase.RemoveAssetBundleName(name, true);
                if (showEditorGUI)
                {
                    counter++;
                    if(length > 100)
                    {
                        counter_t++;
                        if (counter_t >= 30)
                        {
                            counter_t = 0;
                            EditorUtility.DisplayProgressBar("TinaX VFS", $"Remove Assetbundle sign - {counter} / {length}", counter / length);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayProgressBar("TinaX VFS", $"Remove Assetbundle sign - {counter} / {length}", counter / length);
                    }
                }
            }

            if (showEditorGUI)
                EditorUtility.ClearProgressBar();

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        //Source packages 是指 VFS打包资源后的输出目录，里面包括"vfs_root"，"vfs_data"什么的那个目录

        /// <summary>
        /// 检查Source packages的有效性——是否含有mainPakcage的有效信息
        /// </summary>
        /// <returns></returns>
        internal static bool CheckSourcePackagesValid_MainPackage(string root_path)
        {
            //main_package检查
            string main_package_assets_hash_path = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILE_FOLDER_DATA, VFSConst.AssetsHashFileName);
            if (!File.Exists(main_package_assets_hash_path)) return false;

            return true;
        }
        
        internal static bool CheckSourcePackagesValid_AnyExtensionGroups(string root_path)
        {
            var arr = GetValidExtensionGroupNamesFromSourcePackage(root_path);
            return (arr != null && arr.Length > 0);
        }

        internal static bool CheckSourcePackagesValid_ExtensionGroups(string root_path,string extensionGroupName)
        {
            var arr = GetValidExtensionGroupNamesFromSourcePackage(root_path);
            if (arr == null) return false;
            if (arr.Length == 0) return false;
            string g_lower = extensionGroupName.ToLower();
            foreach(var item in arr)
            {
                if (item.ToLower() == g_lower) return true;
            }
            return false;
        }

        internal static string[] GetValidExtensionGroupNamesFromSourcePackage(string root_path)
        {
            List<string> groupNames = new List<string>();
            string extensionGroupRootFolder = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILES_FOLDER_EXTENSION);
            string[] folders = Directory.GetDirectories(extensionGroupRootFolder, "*", SearchOption.TopDirectoryOnly);
            if (folders == null || folders.Length == 0) return Array.Empty<string>();
            foreach(var path in folders)
            {
                string group_name = Path.GetFileName(path);
                bool flag = true;

                if (!File.Exists(Path.Combine(path, VFSConst.VFS_Data_ExtensionGroupInfo_FileName)))
                    flag = false;
                if (!File.Exists(Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILE_FOLDER_DATA, VFSConst.ExtensionGroupAssetsHashFolderName, group_name + ".json")))
                    flag = false;

                if (flag)
                    groupNames.Add(group_name);
            }

            return groupNames.ToArray();
        }

    }
}

