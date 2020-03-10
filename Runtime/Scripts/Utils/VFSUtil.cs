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
        public static void NormalizationConfig(ref VFSConfigModel config)
        {
            if (config == null) return;
            //全局配置

            //后缀名配置 补全缺失的“框架内部定义的必须存在的”配置项
            #region 全局 后缀名 内部配置项
            if (config.GlobalVFS_Ignore_ExtName == null)
                config.GlobalVFS_Ignore_ExtName = new string[0];
            List<string> ext_list = new List<string>();
            foreach (var item in TinaX.VFSKitInternal.InternalVFSConfig.GlobalIgnoreExtName)
            {
                if (!config.GlobalVFS_Ignore_ExtName.Contains(item))
                {
                    ext_list.Add(item);
                }
            }
            if (ext_list.Count > 0)
                ArrayUtil.Combine(ref config.GlobalVFS_Ignore_ExtName, ext_list.ToArray());
            #endregion

            //后缀名配置必须要以点开头，并小写
            #region 全局 后缀名 格式规范
            if (config.GlobalVFS_Ignore_ExtName != null && config.GlobalVFS_Ignore_ExtName.Length > 0)
            {
                for (var i = 0; i < config.GlobalVFS_Ignore_ExtName.Length; i++)
                {
                    if (!config.GlobalVFS_Ignore_ExtName[i].StartsWith("."))
                        config.GlobalVFS_Ignore_ExtName[i] = "." + config.GlobalVFS_Ignore_ExtName[i];

                    config.GlobalVFS_Ignore_ExtName[i] = config.GlobalVFS_Ignore_ExtName[i].ToLower();
                }
            }
            #endregion

            //后缀名配置 重复项
            #region 全局 后缀名 重复项
            if (config.GlobalVFS_Ignore_ExtName != null && config.GlobalVFS_Ignore_ExtName.Length > 0)
            {
                List<string> list = new List<string>(config.GlobalVFS_Ignore_ExtName).Distinct().ToList();
                if (list.Count != config.GlobalVFS_Ignore_ExtName.Length)
                    config.GlobalVFS_Ignore_ExtName = list.ToArray();
            }
            #endregion


            #region 全局 忽略 路径 item

            //补全内部设定
            if (config.GlobalVFS_Ignore_Path_Item == null)
                config.GlobalVFS_Ignore_Path_Item = new string[0];
            List<string> ignore_pathitem_list = new List<string>();
            foreach (var item in TinaX.VFSKitInternal.InternalVFSConfig.GlobalIgnorePathItem)
            {
                if (!config.GlobalVFS_Ignore_Path_Item.Contains(item))
                {
                    ignore_pathitem_list.Add(item);
                }
            }
            if (ignore_pathitem_list.Count > 0)
                ArrayUtil.Combine(ref config.GlobalVFS_Ignore_Path_Item, ignore_pathitem_list.ToArray());

            #endregion

            #region 全局 Assetbundle细节
            if (config.AssetBundleFileExtension.IsNullOrEmpty() || config.AssetBundleFileExtension.IsNullOrWhiteSpace())
                config.AssetBundleFileExtension = InternalVFSConfig.default_AssetBundle_ExtName;

            if (!config.AssetBundleFileExtension.StartsWith("."))
            {
                config.AssetBundleFileExtension = "." + config.AssetBundleFileExtension;
            }

            #endregion

            #region WebVFS
            if(config.DefaultWebVFSBaseUrl.IsNullOrEmpty() || config.DefaultWebVFSBaseUrl.IsNullOrWhiteSpace())
            {
                config.DefaultWebVFSBaseUrl = VFSKit.VFSKit.DefaultDownloadWebAssetUrl;
            }
            if (!config.DefaultWebVFSBaseUrl.EndsWith("/"))
                config.DefaultWebVFSBaseUrl += "/";
            #endregion


                #region 至少包含一个Group
            if (config.Groups == null || config.Groups.Length == 0)
            {
                config.Groups = new VFSGroupOption[]{ VFSGroupOption.New() };
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
        public static bool CheckConfiguration(ref VFSConfigModel config, out VFSErrorCode error, out GroupFolderInfo[] errorfolders)
        {
            errorfolders = null;

            #region 没有任何Group?
            if(config.Groups == null || config.Groups.Length == 0)
            {
                error = VFSErrorCode.NoneGroup;
                return false;
            } 
            #endregion

            #region 检查组名称是否重复
            List<string> temp_list = new List<string>();
            foreach(var group in config.Groups)
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
        public static bool CheckConfig_GroupFolder(ref VFSConfigModel config, out GroupFolderInfo[] folders)
        {
            if (config.Groups == null || config.Groups.Length == 0)
            {
                folders = null;
                return false;
            }
            List<GroupFolderInfo> list_folders = new List<GroupFolderInfo>();

            for (var i = 0; i < config.Groups.Length; i++)
            {
                string group_name = config.Groups[i].GroupName;
                //检查组内部
                if(config.Groups[i].CheckFolderConflict(out var paths))
                {
                    //有冲突
                    paths.ForEach(path => list_folders.Add(new GroupFolderInfo() { FolderPath = path, GroupName = group_name }));
                    folders = list_folders.ToArray();
                    return true;
                }
                
                //检查当前组与之前的组之间是否存在冲突
                if(i > 0)
                {
                    foreach (var path in config.Groups[i].FolderPaths)
                    {
                        for (var j = 0; j < i; j++)
                        {
                            var _g_name = config.Groups[i].GroupName;
                            if (config.Groups[i].CheckFolderConflict(out var result_paths,true))
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
        public static string GetDataFolderInPackages(string packages_root_path)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_DATA);
        }

        /// <summary>
        /// StreamingAssets 里的 vfs_extension
        /// </summary>
        /// <returns></returns>
        public static string GetExtensionGroupRootFolderInStreamingAssets()
        {
            return GetExtensionGroupRootFolderInPackages(GetPackagesRootFolderInStreamingAssets());
        }
        public static string GetExtensionGroupRootFolderInPackages(string packages_root_path)
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
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_EXTENSION, groupName);
        }
        


        public static string GetAssetPathInExtensionGroup(string extension_root_folder,string groupName,string assetbundleName)
        {
            return Path.Combine(extension_root_folder, groupName, assetbundleName);
        }

        public static string GetAssetPath(bool isExtensionGroup, string main_or_extension_folder, string assetBundleName, string group_name = null)
        {
            if (isExtensionGroup)
                return Path.Combine(main_or_extension_folder, group_name, assetBundleName);
            else
                return Path.Combine(main_or_extension_folder, assetBundleName);
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
            return Path.Combine(root_path, VFSConst.VFS_FOLDER_EXTENSION, groupName, VFSConst.AssetBundleFilesHash_FileName);
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
        public static string GetExtensionGroups_AssetBundleManifests_Folder(string packages_root_path, string group_name)
        {
            return Path.Combine(packages_root_path, VFSConst.VFS_FOLDER_EXTENSION, group_name, VFSConst.AssetBundleManifestFileName);
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
            return Path.Combine(package_root_path, VFSConst.VFS_FOLDER_EXTENSION, group, VFSConst.BuildInfoFileName);
        }

        /// <summary>
        /// 在给定的路径下 获取 MainPackage的 版本信息 的路径
        /// </summary>
        /// <param name="package_root_path"></param>
        /// <returns></returns>
        public static string GetMainPackage_VersionInfo_Path(string package_root_path)
        {
            return Path.Combine(package_root_path, VFSConst.VFS_FOLDER_DATA, VFSConst.PakcageVersionFileName);
        }

        /// <summary>
        /// 在给定的路径下 获取 扩展组的 版本信息 的路径
        /// </summary>
        /// <param name="package_root_path"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string GetExtensionGroup_VersionInfo_Path(string package_root_path, string group)
        {
            return Path.Combine(package_root_path, VFSConst.VFS_FOLDER_EXTENSION, group, VFSConst.PakcageVersionFileName);
        }

        private static readonly int head_code = 65279;
        private static readonly char head_char = (char)head_code;
        private static readonly string head_str = new string(new char[1] { head_char });

        public static void RemoveInvalidHead(ref string text)
        {
            if (text.StartsWith(head_str))
                text = text.Substring(1, text.Length - 1);
        }

        public static string RemoveInvalidHead(string text)
        {
            if (text.StartsWith(head_str))
                return text.Substring(1, text.Length - 1);
            else
                return text;
        }

    }

}
