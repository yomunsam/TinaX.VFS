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
using TinaX.VFSKitInternal.Utils;

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

        internal static void InitVFSFoldersInStreamingAssets(string platform,bool clearOtherPlatform)
        {
            string stream_vfs_root_path = Path.Combine(Application.streamingAssetsPath, VFSConst.VFS_STREAMINGASSETS_PATH);
            if (clearOtherPlatform)
            {
                XDirectory.DeleteIfExists(stream_vfs_root_path, true);
                Directory.CreateDirectory(stream_vfs_root_path);
            }
            string stream_platform_root_path = Path.Combine(stream_vfs_root_path,platform);
            XDirectory.DeleteIfExists(stream_platform_root_path, true);
            Directory.CreateDirectory(stream_platform_root_path);

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
        /// 检查在给定的根目录下，是否有有效的MainPackage文件
        /// </summary>
        /// <returns></returns>
        internal static bool IsValid_MainPackage_InPackages(string packages_root_path,bool runtime_used = false)
        {
            //检查是否有vfs_root目录
            string vfs_root = VFSUtil.GetMainPackageFolderInPackages(packages_root_path);
            if (!Directory.Exists(vfs_root)) return false;
            //检查data目录
            if (!Directory.Exists(VFSUtil.GetDataFolderInPackages(packages_root_path))) return false;
            //检查build_info
            if (!File.Exists(VFSUtil.GetMainPackage_BuildInfo_Path(packages_root_path))) return false;
            
            if (!runtime_used)
            {
                //检查asset_hash
                if (!File.Exists(VFSEditorUtil.GetMainPackage_AssetsHash_FilePath_InPackages(packages_root_path))) return false;
                //检查editor build_info
                if (!File.Exists(VFSEditorUtil.Get_EditorBuildInfoPath(packages_root_path))) return false;
            }

            return true;
        }
        
        
        

        /// <summary>
        /// 在给定的根目录下，根据组名检查某个扩展包（组）是否有效
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        internal static bool IsValid_ExtensionGroup_InPackages(string packages_root_path,string group_name)
        {
            var arr = VFSUtil.GetValidExtensionGroupNames(VFSUtil.GetExtensionPackageRootFolderInPackages(packages_root_path));
            if (arr == null || arr.Length ==0) return false;
            return arr.Contains(group_name);
        }

        /// <summary>
        /// 在给定的目录下，是否有任何一个有效的扩展组
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        internal static bool IsAnyValidExtensionGroup_InPackages(string packages_root_path)
        {
            var arr = VFSUtil.GetValidExtensionGroupNames(VFSUtil.GetExtensionPackageRootFolderInPackages(packages_root_path));
            if (arr == null || arr.Length == 0) return false;
            return true;
        }

        public static string GetPackagesRootFolderInStreamingAssets(string platform_name)
        {
            return Path.Combine(Application.streamingAssetsPath, VFSConst.VFS_STREAMINGASSETS_PATH, platform_name);
        }

        /// <summary>
        /// 复制到StreamingAssets
        /// </summary>
        /// <param name="packages_root_path">Packages根目录（有vfs_root,vfs_data之类的那个目录）</param>
        /// <param name="platform_name">平台名</param>
        public static void CopyToStreamingAssets(string packages_root_path, string platform_name, bool clearOtherPlatformFiles = false, bool OnlyMainPackage = false)
        {
            VFSEditorUtil.InitVFSFoldersInStreamingAssets(platform_name, clearOtherPlatformFiles);
            var stream_root_path = Path.Combine(Application.streamingAssetsPath, VFSConst.VFS_STREAMINGASSETS_PATH);
            var project_vfs_root_path = Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_MAIN);
            if (Directory.Exists(project_vfs_root_path))
            {
                string target_vfs_root = Path.Combine(stream_root_path, platform_name, VFSConst.VFS_FOLDER_MAIN);
                XDirectory.DeleteIfExists(target_vfs_root);
                XDirectory.CopyDir(project_vfs_root_path, target_vfs_root);
            }

            if (!OnlyMainPackage)
            {
                var project_vfs_extension_group = Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_EXTENSION);
                if (Directory.Exists(project_vfs_extension_group))
                {
                    string target_vfs_extension = Path.Combine(stream_root_path, platform_name, VFSConst.VFS_FOLDER_EXTENSION);
                    XDirectory.DeleteIfExists(target_vfs_extension);
                    XDirectory.CopyDir(project_vfs_extension_group, target_vfs_extension);
                }
            }

            //Data目录处理----------------------------------------------------------
            //assetBundle hash
            var main_package_assetbundle_hash_files_folder_path = VFSUtil.GetMainPackageAssetBundleHashFilesRootPath(packages_root_path);
            if (Directory.Exists(main_package_assetbundle_hash_files_folder_path))
            {
                string target_path = VFSUtil.GetMainPackageAssetBundleHashFilesRootPath(Path.Combine(stream_root_path, platform_name));
                XDirectory.DeleteIfExists(target_path);
                XDirectory.CopyDir(main_package_assetbundle_hash_files_folder_path, target_path);
            }

            //manifest
            var main_package_manifest_file_folder_path = VFSUtil.GetMainPackage_AssetBundleManifests_Folder(packages_root_path);
            if (Directory.Exists(main_package_manifest_file_folder_path))
            {
                string target_path = VFSUtil.GetMainPackage_AssetBundleManifests_Folder(Path.Combine(stream_root_path, platform_name));
                XDirectory.DeleteIfExists(target_path);
                XDirectory.CopyDir(main_package_manifest_file_folder_path, target_path);
            }
            //build info
            var main_package_build_info = VFSUtil.GetMainPackage_BuildInfo_Path(packages_root_path);
            if (File.Exists(main_package_build_info))
            {
                string target_path = VFSUtil.GetMainPackage_BuildInfo_Path(Path.Combine(stream_root_path, platform_name));
                XFile.DeleteIfExists(target_path);
                XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                File.Copy(main_package_build_info, target_path);
            }

            //vfs config
            var vfs_config_path = VFSUtil.GetVFSConfigFilePath_InPackages(packages_root_path);
            if (File.Exists(vfs_config_path))
            {
                string target_path = VFSUtil.GetVFSConfigFilePath_InPackages(Path.Combine(stream_root_path, platform_name));
                XFile.DeleteIfExists(target_path);
                XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                File.Copy(vfs_config_path, target_path, true);
            }
        }

        public static void CopyExtensionPackageToSreamingAssets(string extension_package_path,string platform,string group_name)
        {
            var target_path = VFSUtil.GetExtensionGroupFolder(VFSUtil.GetPackagesRootFolderInStreamingAssets(platform), group_name);
            XDirectory.DeleteIfExists(target_path);
            Directory.CreateDirectory(target_path);
            XDirectory.CopyDir(extension_package_path, extension_package_path);
        }

        /// <summary>
        /// 根据平台，删除StreamingAsset中的相关Packages
        /// </summary>
        /// <param name="platform_name"></param>
        public static void DeletePackagesFromStreamingAssets(string platform_name)
        {
            string path = GetPackagesRootFolderInStreamingAssets(platform_name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        

        #region Get Paths



        /// <summary>
        /// 获取 Source Package 目录路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetSourcePackagesFolderPath(string platform_name)
        {
            return Path.Combine(VFSEditorConst.PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH, platform_name);
        }

        /// <summary>
        /// 获取 Source Pakcages 下的 Main Package 的 本地文件存放目录 (vfs_root)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_AssetsFolderPath_InSourcePackages(string platform_name)
        {
            return VFSUtil.GetMainPackageFolderInPackages(GetSourcePackagesFolderPath(platform_name));
        }


        /// <summary>
        /// 获取 Source Pakcages 下的 Main Package 的 远程文件存放目录 (vfs_remote)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_RemoteAssetsFolderPath_InSourcePackages(string platform_name)
        {
            return Path.Combine(GetSourcePackagesFolderPath(platform_name), VFSEditorConst.PROJECT_VFS_FILE_FOLDER_REMOTE);
        }

        /// <summary>
        /// 获取 Source Pakcages 下存放打包数据文件的根目录 (vfs_data)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_PackagesDataFolderPath_InSourcePackages(string platform_name)
        {
            return VFSUtil.GetDataFolderInPackages(GetSourcePackagesFolderPath(platform_name));
        }

        /// <summary>
        /// 获取 Source Pakcages 下存放打扩展组资源文件的根目录 (vfs_extension)
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_ExtensionGroupsRootFolder_InSourcePackages(string platform_name)
        {
            return VFSUtil.GetExtensionPackageRootFolderInPackages(GetSourcePackagesFolderPath(platform_name));
        }

        /// <summary>
        /// 获取SourcePackages 下 某个 扩展包（组）的根目录
        /// </summary>
        /// <param name="platform_name"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public static string Get_ExtensionGroupFolderPath_InSourcePackages(string platform_name, string group_name)
        {
            return VFSUtil.GetExtensionGroupFolder(GetSourcePackagesFolderPath(platform_name), group_name);
        }


        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 Assets的Hash记录文件 的路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetMainPackage_AssetsHashFilePath_InSourcePackagesFolder(string platform_name)
        {
            return GetMainPackage_AssetsHash_FilePath_InPackages(GetSourcePackagesFolderPath(platform_name));
        }

        /// <summary>
        /// 在给定的目录下获取 MainPackage 的 原始Assets的hash记录文件 的路径
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string GetMainPackage_AssetsHash_FilePath_InPackages(string packages_root_path)
        {
            return Path.Combine(VFSUtil.GetDataFolderInPackages(packages_root_path), VFSConst.AssetsHashFileName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 扩展组 的 Assets的Hash记录文件 的路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_AssetsHashFilePath_InSourcePackagesFolder(string platform_name,string groupName)
        {
            return Path.Combine(Get_PackagesDataFolderPath_InSourcePackages(platform_name), VFSConst.ExtensionGroupAssetsHashFolderName, groupName + ".json");
        }

        /// <summary>
        /// 获取Source Packages 下 存放 Main Packages 所有的 AssetBundleManifest 的文件的根目录
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string GetMainPackage_AssetBundleManifestsFolderPath_InSourcePackagesFolder(string platform)
        {
            return VFSUtil.GetMainPackage_AssetBundleManifests_Folder(GetSourcePackagesFolderPath(platform));
        }

        /// <summary>
        /// 获取Source Packages 下 扩展组 的 AssetBundleManifest 的文件路径
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_AssetBundleManifestPath_InInSourcePackagesFolder(string platform, string group_name)
        {
            return VFSUtil.GetExtensionGroups_AssetBundleManifests_Folder(GetSourcePackagesFolderPath(platform), group_name);
        }

        /// <summary>
        /// 获取Source Packages 下 存放 Main Packages 所有的 AssetBundle的Hash 的文件的根目录
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string GetMainPackage_AssetBundle_HashFiles_FolderPath_InSourcePackagesFolder(string platform)
        {
            return VFSUtil.GetMainPackageAssetBundleHashFilesRootPath(GetSourcePackagesFolderPath(platform));
        }

        /// <summary>
        /// 获取Source Packages 下 扩展组 的 AssetBundle的Hash 的文件的路径
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_AssetBundle_HashFiles_Path_InInSourcePackagesFolder(string platform, string group_name)
        {
            return VFSUtil.GetExtensionGroup_AssetBundleHashFileFilePath(GetSourcePackagesFolderPath(platform), group_name);
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
        public static string GetVersionDataFolderPath_InProjectVersion(string branchName, long versionCode)
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
        /// 获取 在工程目录里 存储具体某个分支下某个版本的 记录Manifest 文件或文件夹（扩展组是文件，MainPackage是文件夹）的路径
        /// </summary>
        /// <returns></returns>
        public static string GetVersionData_Manifest_FolderOrFilePath(bool isExtensionGroup, string branchName, long versionCode)
        {
            if (isExtensionGroup)
                return Path.Combine(GetVersionDataFolderPath_InProjectVersion(branchName, versionCode), VFSConst.AssetBundleManifestFileName);
            else
                return Path.Combine(GetVersionDataFolderPath_InProjectVersion(branchName, versionCode), VFSConst.MainPackage_AssetBundleManifests_Folder);

        }

        /// <summary>
        /// 获取 在工程目录里 存储具体某个分支下某个版本的 记录AssetBundle文件的Hash值的 文件或文件夹（扩展组是文件，MainPackage是文件夹）的路径
        /// </summary>
        /// <param name="isExtensionGroup"></param>
        /// <param name="branchName"></param>
        /// <param name="versionCode"></param>
        /// <returns></returns>
        public static string GetVersionData_AssetBundle_HashFile_FolderOrFilePath(bool isExtensionGroup,string branchName, long versionCode)
        {
            if (isExtensionGroup)
                return Path.Combine(GetVersionDataFolderPath_InProjectVersion(branchName, versionCode), VFSConst.AssetBundleFilesHash_FileName);
            else
                return Path.Combine(GetVersionDataFolderPath_InProjectVersion(branchName, versionCode), VFSConst.MainPackage_AssetBundle_Hash_Files_Folder);
        }

        public static string GetVersionData_EditorBuildInfo_Path(string branchName, long versionCode)
        {
            return Path.Combine(GetVersionDataFolderPath_InProjectVersion(branchName, versionCode), VFSEditorConst.VFS_EditorBuildInfo_FileName);
        }

        public static string GetVersionData_BuildInfo_Path(string branchName, long versionCode)
        {
            return Path.Combine(GetVersionDataFolderPath_InProjectVersion(branchName, versionCode), VFSConst.BuildInfoFileName);
        }

        /// <summary>
        /// 获取 Source Pakcges 下的 Main Package 的 版本信息 文件路径
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string Get_MainPackage_PackageVersionFilePath_InSourcePackages(string platform_name)
        {
            return Path.Combine(Get_PackagesDataFolderPath_InSourcePackages(platform_name), VFSConst.PakcageVersionFileName);
        }

        

        /// <summary>
        /// 获取 存储在版本记录中的 资源原始二进制文件 文件路径
        /// </summary>
        /// <returns></returns>
        public static string Get_AssetsBinaryFolderPath_InVersion(string branchName, long version )
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
            return Path.Combine(VFSUtil.GetExtensionGroupFolder(VFSEditorUtil.GetSourcePackagesFolderPath(platform_name), groupName), VFSConst.PakcageVersionFileName);
        }

        /// <summary>
        /// 在给定的根目录下 获取 Editor Build Info 的文件路径
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string Get_EditorBuildInfoPath(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_DATA, VFSEditorConst.VFS_EditorBuildInfo_FileName);
        }
        
        public static string Get_FileServer_Default_RootFolderPath()
        {
            return VFSEditorConst.PROJECT_VFS_FILES_SERVER_FOLDER_PATH;
        }

        #endregion

    }
}

