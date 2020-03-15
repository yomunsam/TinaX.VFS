using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TinaX.VFSKit;
using UnityEngine;
using TinaX.VFSKit.Const;
using TinaX.Utils;

namespace TinaX.VFSKitInternal.Utils
{
    public static class VFSUtil
    {

        /// <summary>
        /// 是否为子路径， 如果相等也返回false
        /// </summary>
        /// <param name="path1">路径1</param>
        /// <param name="path2">路径2</param>
        /// <param name="mutual">是否互相检测，如果为false，只检查path1是否为path2的子路径；如果为true，互相判断两者是否为对方的子路径</param>
        /// <returns></returns>
        public static bool IsSubpath(string path1, string path2, bool mutual = false)
        {
            string p1 = (path1.EndsWith("/") || path1.EndsWith("\\")) ? path1.Replace("\\", "/") : path1.Replace("\\", "/") + "/";
            string p2 = (path2.EndsWith("/") || path2.EndsWith("\\")) ? path2.Replace("\\", "/") : path2.Replace("\\", "/") + "/";

            if (p1 == p2) return false;

            if (mutual)
            {
                //相互判断
                return (p1.StartsWith(p2) || p2.StartsWith(p1));
            }
            else
            {
                //判断p1是否是p2的子路径
                return p1.StartsWith(p2);
            }

        }


        /// <summary>
        /// 是否为子路径或相同路径
        /// </summary>
        /// <param name="path1">路径1</param>
        /// <param name="path2">路径2</param>
        /// <param name="mutual">是否互相检测，如果为false，只检查path1是否为path2的子路径；如果为true，互相判断两者是否为对方的子路径</param>
        /// <returns></returns>
        public static bool IsSameOrSubPath(string path1, string path2, bool mutual = false)
        {
            string p1 = (path1.EndsWith("/") || path1.EndsWith("\\")) ? path1.Replace("\\", "/") : path1.Replace("\\", "/") + "/";
            string p2 = (path2.EndsWith("/") || path2.EndsWith("\\")) ? path2.Replace("\\", "/") : path2.Replace("\\", "/") + "/";

            if (p1 == p2) return true;

            if (mutual)
            {
                //相互判断
                return (p1.StartsWith(p2) || p2.StartsWith(p1));
            }
            else
            {
                //判断p1是否是p2的子路径
                return p1.StartsWith(p2);
            }
        }

        public static bool IsInResources(string path)
        {
            return path.Replace('\\', '/').ToLower().Contains("/resources/");
        }


        /// <summary>
        /// 配置规范化
        /// </summary>
        /// <param name="config"></param>
        public static void NormalizationConfig(IVFSConfig config)
        {
            if (config == null) return;
            //全局配置

            //后缀名配置 补全缺失的“框架内部定义的必须存在的”配置项
            #region 全局 后缀名 内部配置项
            if (config.PGlobalVFS_Ignore_ExtName == null)
                config.PGlobalVFS_Ignore_ExtName = new string[0];
            List<string> ext_list = new List<string>();
            foreach (var item in TinaX.VFSKitInternal.InternalVFSConfig.GlobalIgnoreExtName)
            {
                if (!config.PGlobalVFS_Ignore_ExtName.Contains(item))
                {
                    ext_list.Add(item);
                }
            }
            if (ext_list.Count > 0)
                config.PGlobalVFS_Ignore_ExtName = ArrayUtil.Combine(config.PGlobalVFS_Ignore_ExtName, ext_list.ToArray());
            #endregion

            //后缀名配置必须要以点开头，并小写
            #region 全局 后缀名 格式规范
            if (config.PGlobalVFS_Ignore_ExtName != null && config.PGlobalVFS_Ignore_ExtName.Length > 0)
            {
                for (var i = 0; i < config.PGlobalVFS_Ignore_ExtName.Length; i++)
                {
                    if (!config.PGlobalVFS_Ignore_ExtName[i].StartsWith("."))
                        config.PGlobalVFS_Ignore_ExtName[i] = "." + config.PGlobalVFS_Ignore_ExtName[i];

                    config.PGlobalVFS_Ignore_ExtName[i] = config.PGlobalVFS_Ignore_ExtName[i].ToLower();
                }
            }
            #endregion

            //后缀名配置 重复项
            #region 全局 后缀名 重复项
            if (config.PGlobalVFS_Ignore_ExtName != null && config.PGlobalVFS_Ignore_ExtName.Length > 0)
            {
                List<string> list = new List<string>(config.PGlobalVFS_Ignore_ExtName).Distinct().ToList();
                if (list.Count != config.PGlobalVFS_Ignore_ExtName.Length)
                    config.PGlobalVFS_Ignore_ExtName = list.ToArray();
            }
            #endregion


            #region 全局 忽略 路径 item

            //补全内部设定
            if (config.PGlobalVFS_Ignore_Path_Item == null)
                config.PGlobalVFS_Ignore_Path_Item = new string[0];
            List<string> tmp_path_item_list = new List<string>(config.PGlobalVFS_Ignore_Path_Item);
            foreach (var item in TinaX.VFSKitInternal.InternalVFSConfig.GlobalIgnorePathItem)
            {
                if (!tmp_path_item_list.Contains(item))
                {
                    tmp_path_item_list.Add(item);
                }
            }
            List<string> temp_2 = new List<string>();
            //查重和空白项目
            for(int i = tmp_path_item_list.Count -1; i >= 0; i--)
            {
                if (!tmp_path_item_list[i].IsNullOrEmpty())
                {
                    if(!temp_2.Any(item => item.ToLower() == tmp_path_item_list[i].ToLower()))
                    {
                        temp_2.Add(tmp_path_item_list[i]);
                    }
                }
            }

            config.PGlobalVFS_Ignore_Path_Item = temp_2.ToArray();


            #endregion

                #region 全局 Assetbundle细节
            if (config.PAssetBundleFileExtension.IsNullOrEmpty() || config.PAssetBundleFileExtension.IsNullOrWhiteSpace())
                config.PAssetBundleFileExtension = InternalVFSConfig.default_AssetBundle_ExtName;

            if (!config.PAssetBundleFileExtension.StartsWith("."))
            {
                config.PAssetBundleFileExtension = "." + config.PAssetBundleFileExtension;
            }
            config.PAssetBundleFileExtension = config.PAssetBundleFileExtension.ToLower();

            #endregion

            #region WebVFS
            if (config.PDefaultWebVFSBaseUrl.IsNullOrEmpty() || config.PDefaultWebVFSBaseUrl.IsNullOrWhiteSpace())
            {
                config.PDefaultWebVFSBaseUrl = VFSKit.VFSKit.DefaultDownloadWebAssetUrl;
            }
            if (!config.PDefaultWebVFSBaseUrl.EndsWith("/"))
                config.PDefaultWebVFSBaseUrl += "/";
            #endregion


                #region 至少包含一个Group
            if (config.PGroups == null || config.PGroups.Length == 0)
            {
                config.PGroups = new VFSGroupOption[]{ VFSGroupOption.New() };
            }
            #endregion

        }

        


        /// <summary>
        /// 检查配置 （检查所有项目）, 如果检查通过返回true, 否则返回false
        /// </summary>
        /// <param name="config"></param>
        /// <param name=""></param>
        /// <param name="errorfolders">如果有group内folder相关的错误，则在此处体现</param>
        /// <returns>if error ,return false</returns>
        public static bool CheckConfiguration(IVFSConfig config, out VFSErrorCode error, out GroupFolderInfo[] errorfolders)
        {
            errorfolders = null;

            #region 没有任何Group?
            if(config.PGroups == null || config.PGroups.Length == 0)
            {
                error = VFSErrorCode.NoneGroup;
                return false;
            } 
            #endregion

            #region 检查组名称是否重复
            List<string> temp_list = new List<string>();
            foreach(var group in config.PGroups)
            {
                if (temp_list.Contains(group.GroupName))
                {
                    //存在重复命名
                    error = VFSErrorCode.SameGroupName;
                    return false;
                }
                else
                    temp_list.Add(group.GroupName);
            }


            #endregion

            #region 检查各组的folder配置是否冲突
            if(CheckConfig_GroupFolder(ref config, out var folders))
            {
                errorfolders = folders;
                error = VFSErrorCode.ConfigureGroupsConflict;
                return false;
            }

            #endregion

            error = default;
            return true;
        }


        /// <summary>
        /// 配置检查： 检查各组的Folder是否有冲突, 如果有冲突，返回true
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool CheckConfig_GroupFolder(ref IVFSConfig config, out GroupFolderInfo[] folders)
        {
            if (config.PGroups == null || config.PGroups.Length == 0)
            {
                folders = null;
                return false;
            }
            List<GroupFolderInfo> list_folders = new List<GroupFolderInfo>();

            for (var i = 0; i < config.PGroups.Length; i++)
            {
                string group_name = config.PGroups[i].GroupName;
                //检查组内部
                if(config.PGroups[i].CheckFolderConflict(out var paths))
                {
                    //有冲突
                    paths.ForEach(path => list_folders.Add(new GroupFolderInfo() { FolderPath = path, GroupName = group_name }));
                    folders = list_folders.ToArray();
                    return true;
                }
                
                //检查当前组与之前的组之间是否存在冲突
                if(i > 0)
                {
                    foreach (var path in config.PGroups[i].FolderPaths)
                    {
                        for (var j = 0; j < i; j++)
                        {
                            var _g_name = config.PGroups[i].GroupName;
                            if (config.PGroups[i].CheckFolderConflict(out var result_paths,true))
                            {
                                //有冲突
                                list_folders.Add(new GroupFolderInfo() { GroupName = group_name, FolderPath = path });
                                result_paths.ForEach(r_path => list_folders.Add(new GroupFolderInfo() { FolderPath = r_path, GroupName = _g_name }));
                                folders = list_folders.ToArray();
                                return true;
                            }
                        }
                    }
                }
            }
            folders = null;
            return false;

        }

       
        /// <summary>
        /// 判断给定的路径是否是个有效的扩展包（每个扩展组是一个独立的扩展包）
        /// </summary>
        /// <param name="package_path"></param>
        /// <returns></returns>
        public static bool IsValidExtensionPackage(string package_path)
        {
            if (!Directory.Exists(package_path)) return false;
            //检查，扩展包的根目录下会有的几个东西
            //FileHash
            string hash_path = Path.Combine(package_path, VFSConst.AssetBundleFilesHash_FileName);
            if (!File.Exists(hash_path)) return false;
            //group_option
            if (!File.Exists(VFSUtil.Get_GroupOptions_InExtensionPackage(package_path))) return false;
            //groupInfo
            if (!File.Exists(VFSUtil.GetExtensionGroup_GroupInfo_Path_InGroupPath(package_path))) return false;
            //manifest
            if (!File.Exists(Path.Combine(package_path, VFSConst.AssetBundleManifestFileName))) return false;

            return true;
        }

        /// <summary>
        /// 在给定的目录下，寻找有效可用的扩展组信息，返回组数组
        /// </summary>
        /// <param name="extensions_root_path">存放扩展组的根目录，该方法只会搜寻它的直接子目录</param>
        /// <param name="main_package_version">如果提供Main Package版本信息，将只会返回满足扩展组对MainPackage最低版本限制的扩展组列表</param>
        /// <returns>组名 数组</returns>
        public static string[] GetValidExtensionGroupNames(string extensions_root_path, long? main_package_version = null)
        {
            if (!Directory.Exists(extensions_root_path)) return Array.Empty<string>();
            List<string> groups = new List<string>();
            string[] folders = Directory.GetDirectories(extensions_root_path, "*", SearchOption.TopDirectoryOnly);
            foreach (var folder in folders)
            {
                if (VFSUtil.IsValidExtensionPackage(folder))
                {
                    //读取groupName
                    try
                    {
                        string group_info_path = Path.Combine(folder, VFSConst.VFS_ExtensionGroupInfo_FileName);
                        string group_info_json = File.ReadAllText(group_info_path);
                        var group_info_obj = JsonUtility.FromJson<ExtensionGroupInfo>(group_info_json);
                        if (main_package_version != null)
                        {
                            //判断版本
                            if (main_package_version.Value >= group_info_obj.MainPackageVersionLimit)
                                groups.Add(group_info_obj.GroupName);
                        }
                        else
                            groups.Add(group_info_obj.GroupName);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[TinaX.VFS] Exception while load extension package info : " + folder + "\n" + e.Message);
                    }
                }
            }
            return groups.ToArray();
        }


        /// <summary>
        /// 获取扩展组的那个文件夹的名字，（不是完整路径）
        /// </summary>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public static string GetExtensionGroupFolderName(string group_name)
        {
            return group_name.ToLower();
        }

        /// <summary>
        /// 获取StreamingAssets下的存放packages的根目录 （eg: streamingassets/TinaX_VFS/android）
        /// </summary>
        /// <param name="platform_name"></param>
        /// <returns></returns>
        public static string GetPackagesRootFolderInStreamingAssets(string platform_name)
        {
            return Path.Combine(Application.streamingAssetsPath, VFSConst.VFS_STREAMINGASSETS_PATH, platform_name);
        }
        public static string GetPackagesRootFolderInStreamingAssets()
        {
            var pname = XPlatformUtil.GetNameText(XPlatformUtil.GetXRuntimePlatform(Application.platform));
            return GetPackagesRootFolderInStreamingAssets(pname);
        }

        /// <summary>
        /// StreamingAssets 里的 vfs_root
        /// </summary>
        /// <returns></returns>
        public static string GetMainPackageFolderInStreamingAssets()
        {
            return GetMainPackageFolderInPackages(GetPackagesRootFolderInStreamingAssets());
        }
        /// <summary>
        /// 给定路径里的main package 路径（vfs_root)
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string GetMainPackageFolderInPackages(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_MAIN);
        }

        /// <summary>
        /// StreamingAssets 里的 vfs_data
        /// </summary>
        /// <returns></returns>
        public static string GetDataFolderInStreamingAssets()
        {
            return GetDataFolderInPackages(GetPackagesRootFolderInStreamingAssets());
        }
        /// <summary>
        /// 获取给定路径里的data目录(vfs_data) 
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string GetDataFolderInPackages(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_DATA);
        }

        /// <summary>
        /// StreamingAssets 里的 vfs_extension
        /// </summary>
        /// <returns></returns>
        public static string GetExtensionPackageRootFolderInStreamingAssets()
        {
            return GetExtensionPackageRootFolderInPackages(GetPackagesRootFolderInStreamingAssets());
        }
        /// <summary>
        /// 在给定的根目录中，获取存放扩展包的文件夹的路径（vfs_extension)
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string GetExtensionPackageRootFolderInPackages(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_EXTENSION);
        }

        /// <summary>
        /// 在给定的根目录中，获取到某个扩展组的根目录
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static string GetExtensionGroupFolder(string packages_root_path, string groupName)
        {
            return Path.Combine(VFSUtil.GetExtensionPackageRootFolderInPackages(packages_root_path), GetExtensionGroupFolderName(groupName));
        }
        

        /// <summary>
        /// 在给定的“扩展包根目录”中，获取某个扩展包中的assetbundle的路径
        /// </summary>
        /// <param name="extension_root_folder"></param>
        /// <param name="groupName"></param>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public static string GetAssetBundlePathFromExtensionGroup_InExtensionPackagesRootFolder(string extension_root_folder,string groupName,string assetbundleName)
        {
            return Path.Combine(extension_root_folder, GetExtensionGroupFolderName(groupName), assetbundleName);
        }

        /// <summary>
        /// 获取给定的根目录中（Packages root path），某个AssetBundle的位置
        /// </summary>
        /// <param name="isExtensionGroup"></param>
        /// <param name="packages_root_path"></param>
        /// <param name="assetBundleName"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public static string GetAssetBundlePathFromPackages(bool isExtensionGroup, string packages_root_path, string assetBundleName, string group_name = null)
        {
            if (isExtensionGroup)
                return GetAssetBundlePathFromExtensionGroup_InExtensionPackagesRootFolder(GetExtensionPackageRootFolderInPackages(packages_root_path), group_name, assetBundleName);
            else
                return Path.Combine(VFSUtil.GetMainPackageFolderInPackages(packages_root_path), assetBundleName);
            
        }

        /// <summary>
        /// 获取Main Package中某个组的AssetBundle的Hash记录文件
        /// </summary>
        /// <param name="root_path"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static string GetMainPackageAssetBundleHashFilePath(string root_path, string groupName)
        {
            return Path.Combine(GetMainPackageAssetBundleHashFilesRootPath(root_path), groupName.GetMD5(true,true));
        }

        /// <summary>
        /// 获取Main Package中所有AssetBundle的hash记录文件存放的根目录（vfs_data/main_hashs/）
        /// </summary>
        /// <param name="root_path"></param>
        /// <returns></returns>
        public static string GetMainPackageAssetBundleHashFilesRootPath(string root_path)
        {
            return Path.Combine(root_path, VFSConst.VFS_FOLDER_DATA, VFSConst.MainPackage_AssetBundle_Hash_Files_Folder);
        }

        /// <summary>
        /// 获取给定的根目录中  扩展组的AssetBundle的 Hash 记录文件
        /// </summary>
        /// <param name="root_path"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_AssetBundleHashFileFilePath(string root_path,string groupName)
        {
            return Path.Combine(root_path, VFSConst.VFS_FOLDER_EXTENSION, GetExtensionGroupFolderName(groupName), VFSConst.AssetBundleFilesHash_FileName);
        }

        /// <summary>
        /// 在给定的跟路径下获取记录MainPackage存放所有AssetBundleManifest文件的目录
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string GetMainPackage_AssetBundleManifests_Folder(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_DATA, VFSConst.MainPackage_AssetBundleManifests_Folder);
        }

        /// <summary>
        /// 在给定的路径下获取记录 扩展组 的 AssetBundleManifest 的路径
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <param name="group_name"></param>
        /// <returns></returns>
        public static string GetExtensionGroups_AssetBundleManifests_FilePath(string packages_root_path, string group_name)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_EXTENSION, GetExtensionGroupFolderName(group_name), VFSConst.AssetBundleManifestFileName);
        }

        /// <summary>
        /// 在给定的路径下 获取 MainPackage的 BuildInfo 的路径
        /// </summary>
        /// <param name="package_root_path"></param>
        /// <returns></returns>
        public static string GetMainPackage_BuildInfo_Path(string package_root_path)
        {
            return Path.Combine(package_root_path, VFSConst.VFS_FOLDER_DATA, VFSConst.BuildInfoFileName);
        }

        /// <summary>
        /// 在给定的路径下 获取 扩展组的 BuildInfo 的路径
        /// </summary>
        /// <param name="package_root_path"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_BuildInfo_Path(string package_root_path, string group)
        {
            return GetExtensionGroup_BuildInfo_Path_InGroupPath(GetExtensionGroupFolder(package_root_path, group));
        }

        /// <summary>
        /// 在给定的 扩展组的根目录下 获取扩展组的 BuildInfo 文件路径
        /// </summary>
        /// <param name="group_path"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_BuildInfo_Path_InGroupPath(string group_path)
        {
            return Path.Combine(group_path, VFSConst.BuildInfoFileName);
        }

        /// <summary>
        /// 在给定的 扩展组的根目录下 获取扩展组的 GroupInfo 文件路径
        /// </summary>
        /// <param name="group_path"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_GroupInfo_Path_InGroupPath(string group_path)
        {
            return Path.Combine(group_path, VFSConst.VFS_ExtensionGroupInfo_FileName);
        }

        /// <summary>
        /// 在给定的路径下 获取 MainPackage的 版本信息 的路径
        /// </summary>
        /// <param name="package_root_path"></param>
        /// <returns></returns>
        public static string GetMainPackage_VersionInfo_Path(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_DATA, VFSConst.PakcageVersionFileName);
        }

        /// <summary>
        /// 在给定的根目录中获取扩展组的option文件路径
        /// </summary>
        public static string GetExtensionPackages_GroupOptions_FilePath(string packages_root_path, string group_name)
        {
            return Get_GroupOptions_InExtensionPackage(GetExtensionGroupFolder(packages_root_path, group_name));
        }

        /// <summary>
        /// 在给定的扩展包（组）的根目录中，获取GroupOptions文件的路径
        /// </summary>
        /// <param name="extension_group_path"></param>
        /// <returns></returns>
        public static string Get_GroupOptions_InExtensionPackage(string extension_group_path)
        {
            return Path.Combine(extension_group_path, VFSConst.ExtensionGroup_GroupOption_FileName);
        }

        /// <summary>
        /// 在给定的路径下 获取 扩展组的 版本信息 的路径
        /// </summary>
        /// <param name="package_root_path"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_VersionInfo_Path(string package_root_path, string group)
        {
            return Path.Combine(package_root_path, VFSConst.VFS_FOLDER_EXTENSION, GetExtensionGroupFolderName(group), VFSConst.PakcageVersionFileName);
        }

        /// <summary>
        /// 获取Runtime下的VFS主Config文件路径
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public static string GetVFSConfigFilePath_InPackages(string packages_root_path)
        {
            return Path.Combine(GetDataFolderInPackages(packages_root_path), VFSConst.Config_Runtime_FileName);
        }


        public static string GetMainPackage_UpgradableVersionFilePath(string packages_root_path)
        {
            return Path.Combine(GetDataFolderInPackages(packages_root_path), VFSConst.Upgradable_Vesion_FileName);
        }

    }

}
