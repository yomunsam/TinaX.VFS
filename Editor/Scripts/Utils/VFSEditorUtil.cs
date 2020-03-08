using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using TinaX;
using TinaX.Utils;
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
            string project_root_path = Application.streamingAssetsPath;
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

        #region Get Paths

        /// <summary>
        /// 获取 Source Package 目录路径
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string GetSourcePackagesFolderPath(ref XRuntimePlatform platform)
        {
            string platform_name = XPlatformUtil.GetNameText(platform);
            return GetSourcePackagesFolderPath(ref platform_name);
        }

        /// <summary>
        /// 获取 Source Package 目录路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetSourcePackagesFolderPath(ref string platform_name)
        {
            return Path.Combine(VFSEditorConst.PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH, platform_name);
        }

        /// <summary>
        /// 获取 Source Pakcages 下的 Main Package 的 本地文件存放目录 (vfs_root)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_LocalAssetsFolderPath_InSourcePackages(ref string platform_name)
        {
            return Path.Combine(GetSourcePackagesFolderPath(ref platform_name), VFSEditorConst.PROJECT_VFS_FILES_FOLDER_MAIN);
        }

        /// <summary>
        /// 获取 Source Pakcages 下的 Main Package 的 本地文件存放目录 (vfs_root)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_LocalAssetsFolderPath_InSourcePackages(ref XRuntimePlatform platform)
        {
            string platform_name = XPlatformUtil.GetNameText(platform);
            return Get_MainPackage_LocalAssetsFolderPath_InSourcePackages(ref platform_name);
        }


        /// <summary>
        /// 获取 Source Pakcages 下的 Main Package 的 远程文件存放目录 (vfs_remote)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_RemoteAssetsFolderPath_InSourcePackages(ref string platform_name)
        {
            return Path.Combine(GetSourcePackagesFolderPath(ref platform_name), VFSEditorConst.PROJECT_VFS_FILE_FOLDER_REMOTE);
        }

        /// <summary>
        /// 获取 Source Pakcages 下的 Main Package 的 远程文件存放目录 (vfs_remote)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_RemoteAssetsFolderPath_InSourcePackages(ref XRuntimePlatform platform)
        {
            string platform_name = XPlatformUtil.GetNameText(platform);
            return Get_MainPackage_RemoteAssetsFolderPath_InSourcePackages(ref platform_name);
        }


        /// <summary>
        /// 获取 Source Pakcages 下存放打包数据文件的根目录 (vfs_data)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_PackagesDataFolderPath_InSourcePackages(ref string platform_name)
        {
            return Path.Combine(GetSourcePackagesFolderPath(ref platform_name), VFSEditorConst.PROJECT_VFS_FILE_FOLDER_DATA);
        }

        /// <summary>
        /// 获取 Source Pakcages 下存放打扩展组资源文件的根目录 (vfs_extension)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_ExtensionGroupsRootFolder_InSourcePackages(ref string platform_name)
        {
            return Path.Combine(GetSourcePackagesFolderPath(ref platform_name), VFSEditorConst.PROJECT_VFS_FILES_FOLDER_EXTENSION);
        }

        public static string Get_ExtensionGroupFolderPath_InSourcePackages(ref string platform_name, ref string groupName)
        {
            return Path.Combine(Get_ExtensionGroupsRootFolder_InSourcePackages(ref platform_name), groupName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 Manifest文件路径
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string GetMainPackageManifestFilePathInSourcePackagesFolder(ref XRuntimePlatform platform)
        {
            string platform_name = XPlatformUtil.GetNameText(platform);
            return GetMainPackageManifestFilePathInSourcePackagesFolder(ref platform_name);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 Manifest文件路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetMainPackageManifestFilePathInSourcePackagesFolder(ref string platform_name)
        {
            return Path.Combine(Get_MainPackage_LocalAssetsFolderPath_InSourcePackages(ref platform_name), VFSConst.AssetsManifestFileName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 Assets的Hash记录文件 的路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetMainPackage_AssetsHashFilePath_InSourcePackagesFolder(ref string platform_name)
        {
            return Path.Combine(Get_PackagesDataFolderPath_InSourcePackages(ref platform_name), VFSConst.AssetsHashFileName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 Assets的Hash记录文件 的路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_AssetsHashFilePath_InSourcePackagesFolder(ref string platform_name,ref string groupName)
        {
            return Path.Combine(Get_PackagesDataFolderPath_InSourcePackages(ref platform_name), VFSConst.ExtensionGroupAssetsHashFolderName, groupName + ".json");
        }

        /// <summary>
        /// 获取 Source Package 下 扩展组 的 Manifest 文件 的路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_ManifestFilePath_IsSourcePackagesFolder(ref string platform_name,ref string groupName)
        {
            return Path.Combine(Get_ExtensionGroupsRootFolder_InSourcePackages(ref platform_name), groupName, VFSConst.AssetsManifestFileName);
        }

        /// <summary>
        /// 获取在项目工程里 存储版本信息的文件夹的 根目录路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectVersionRootFolderPath()
        {
            return VFSEditorConst.VFS_VERSION_ROOT_FOLDER_PATH;
        }

        /// <summary>
        /// 获取 存储版本信息的文件夹下 /Data 路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectVersionDataFolderPath() => VFSEditorConst.VFS_VERSION_RECORD_Data_FOLDER_PATH;

        /// <summary>
        /// 获取 在工程目录里 存储具体某个分支下某个版本的记录数据 的 目录，如："VFS_Version/Data/Branches/master/1/"
        /// </summary>
        /// <param name="branchName"></param>
        /// <param name="versionCode"></param>
        /// <returns></returns>
        public static string GetVersionDataFolderPath_InProjectVersion(ref string branchName, ref long versionCode)
        {
            return Path.Combine(GetVersionDataRootFolderPath_InProjectVersion(ref branchName), versionCode.ToString());
        }

        /// <summary>
        /// 获取 在工程目录里 存储具体某个分支所有版本的数据记录的 根目录，如："VFS_Version/Data/Branches/master/"
        /// </summary>
        /// <param name="branchName"></param>
        /// <param name="versionCode"></param>
        /// <returns></returns>
        public static string GetVersionDataRootFolderPath_InProjectVersion(ref string branchName)
        {
            return Path.Combine(GetProjectVersionDataFolderPath(), "Branches", branchName);
        }

        /// <summary>
        /// 获取 存储在版本记录中的 Manifest 文件路径
        /// </summary>
        /// <param name="branchName"></param>
        /// <param name="versionCode"></param>
        /// <returns></returns>
        public static string GetManifestFilePathInVersionData(ref string branchName, ref long versionCode)
        {
            return Path.Combine(GetProjectVersionDataFolderPath(), "Branches", branchName, versionCode.ToString(), VFSConst.AssetsManifestFileName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 版本信息 文件路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_PackageVersionFilePath_InSourcePackages(ref string platform_name)
        {
            return Path.Combine(Get_PackagesDataFolderPath_InSourcePackages(ref platform_name), VFSConst.PakcageVersionFileName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 版本信息 文件路径
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string Get_MainPackage_PackageVersionFilePath_InSourcePackages(ref XRuntimePlatform platform)
        {
            string platform_name = XPlatformUtil.GetNameText(platform);
            return Get_MainPackage_PackageVersionFilePath_InSourcePackages(ref platform_name);
        }

        /// <summary>
        /// 获取 存储在版本记录中的 资源原始二进制文件 文件路径
        /// </summary>
        /// <returns></returns>
        public static string Get_AssetsBinaryFolderPath_InVersion(ref string branchName, ref long version )
        {
            return Path.Combine(GetProjectVersionRootFolderPath(), VFSEditorConst.VFS_VERSION_RECORD_Binary_FOLDER_PATH, "Branches", branchName, version.ToString());
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 扩展组 的 版本信息 文件路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_ExtensionGroups_PackageVersionFilePath_InSourcePackages(ref string platform_name,ref string groupName)
        {
            //这是Group的【会在Runtime下用到的】文件，所以不放在vfs_root下，直接放在Group的文件里
            return Path.Combine(Get_ExtensionGroupsRootFolder_InSourcePackages(ref platform_name), groupName, VFSConst.PakcageVersionFileName);
        }

        #endregion

    }
}

