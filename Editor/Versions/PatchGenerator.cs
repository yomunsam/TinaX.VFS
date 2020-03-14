using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.Utils;
using TinaX.VFSKit;
using TinaXEditor.VFSKit.Versions;
using TinaXEditor.VFSKit.Utils;
using TinaXEditor.VFSKitInternal;
using TinaX.VFSKitInternal.Utils;
using TinaX.VFSKit.Const;
using TinaXEditor.VFSKit.Const;
using UnityEngine;
using TinaX.VFSKitInternal;
using UnityEditor;

namespace TinaXEditor.VFSKit
{
    public class PatchGenerator : IPathchGenerator
    {
        public bool EnableEditorGUI { get; set; } = true;
        public bool EnableLog { get; set; } = false;

        public void MakePatchFromVersionLibrary(string branchName, long current_versionCode, long target_versionCode, string output_path)
        {
            if(current_versionCode <= target_versionCode)
            {
                Debug.LogError($"[TinaX.VFS][PatchGenerator] Failed to make patch: current versionCode must greater than target versionCode");
                return;
            }
            //检查分支和版本是否存在
            if(VFSManagerEditor.VersionManager.TryGetVersionBranch(branchName,out var branch))
            {
                //检查版本
                VersionRecord record_current;
                VersionRecord record_target;
                if(branch.TryGetVersion(current_versionCode, out record_current) && branch.TryGetVersion(target_versionCode, out record_target))
                {
                    //读取到两者的数据,读取两者的记录

                    string platform_name = TinaX.Utils.XPlatformUtil.GetNameText(branch.Platform);
                    string target_data_root_path = VFSEditorUtil.GetVersionDataFolderPath_InProjectVersion(branchName, target_versionCode);
                    string current_data_root_path = VFSEditorUtil.GetVersionDataFolderPath_InProjectVersion(branchName, current_versionCode);

                    string current_binary_path = VFSEditorUtil.Get_AssetsBinaryFolderPath_InVersion(branchName, current_versionCode);
                    string current_binary_zip_path = Path.Combine(current_binary_path, VFSEditorConst.VFS_VERSION_AssetsBinary_Zip_Name);
                    bool current_zip_file_not_found = false;
                    bool UseSourcePackagesFolder = false;
                    if (!File.Exists(current_binary_zip_path))
                    {
                        current_zip_file_not_found = true;
                        //文件不存在，检查sourcepackages那边的文件能不能用
                        if (this.IsSourcePackageFilesCanUse(ref branch, ref record_current, platform_name))
                        {
                            //能用
                            UseSourcePackagesFolder = true;
                        }
                        else
                            UseSourcePackagesFolder = false;
                    }
                    if (current_zip_file_not_found)
                    {
                        if (UseSourcePackagesFolder)
                        {
                            makePatchBySourcePackages(ref branch, ref record_target, current_versionCode, target_data_root_path, Path.GetDirectoryName(output_path), output_path);
                        }
                        else
                        {
                            Debug.LogError(IsChinese ? "当前版本的记录中并没有存档二进制文件，所以无法制作补丁。" : "Binaries are not archived in the records of the current version, so patches cannot be made.");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[TinaX.VFS][PatchGenerator] Failed to make patch: target versionCode or current versionCode is invalid. ");
                }
            }
            else
            {
                Debug.LogError($"[TinaX.VFS][PatchGenerator] Failed to make patch: branch name \"{branchName}\" is invalid. ");
            }
        }





        /// <summary>
        /// 判断：是否能用source packages中的资源与给定的build_id是否相同
        /// </summary>
        /// <returns></returns>
        private bool IsSourcePackageFilesCanUse(ref VersionBranch branch,ref VersionRecord record, string platform_name)
        {
            string source_packages_root_path = VFSEditorUtil.GetSourcePackagesFolderPath(platform_name);
            if (!Directory.Exists(source_packages_root_path)) return false;
            string build_info_source = (branch.BType == VersionBranch.BranchType.MainPackage) ? VFSUtil.GetMainPackage_BuildInfo_Path(source_packages_root_path) : VFSUtil.GetExtensionGroup_BuildInfo_Path(source_packages_root_path, branch.ExtensionGroupName);
            if (!File.Exists(build_info_source)) return false;
            try
            {
                var build_info_obj = XConfig.GetJson<TinaX.VFSKitInternal.BuildInfo>(build_info_source, AssetLoadType.SystemIO, false);
                return (record.build_id == build_info_obj.BuildID);
            }
            catch
            {
                return false;
            }
        }

        private void makePatchBySourcePackages(ref VersionBranch branch, ref VersionRecord target_record, long cur_version_code, string target_data_root_path,string save_folder,string save_file_name)
        {
            string platform_name = TinaX.Utils.XPlatformUtil.GetNameText(branch.Platform);
            string source_packages_root_folder = VFSEditorUtil.GetSourcePackagesFolderPath(platform_name);
            string target_asset_hash_path = Path.Combine(target_data_root_path, VFSConst.AssetsHashFileName);
            string asset_hash_path_source = (branch.BType == VersionBranch.BranchType.MainPackage) ? VFSEditorUtil.GetMainPackage_AssetsHashFilePath_InSourcePackagesFolder(platform_name) : VFSEditorUtil.GetExtensionGroup_AssetsHashFilePath_InSourcePackagesFolder(platform_name, branch.ExtensionGroupName);
            
            if (!File.Exists(target_asset_hash_path)) Debug.LogError($"[TinaX.VFS][PatchGenerator] The record corresponding to the target version is lost. ");
            if (!File.Exists(asset_hash_path_source)) Debug.LogError($"[TinaX.VFS][PatchGenerator] The record corresponding to the source packages is lost. ");

            var asset_hash_obj_target = XConfig.GetJson<FilesHashBook>(target_asset_hash_path, AssetLoadType.SystemIO, false);
            var asset_hash_obj_source = XConfig.GetJson<FilesHashBook>(asset_hash_path_source, AssetLoadType.SystemIO, false);

            PatchRecords records = new PatchRecords();

            if(branch.BType == VersionBranch.BranchType.MainPackage)
            {
                var source_vfsconfig = XConfig.GetJson<VFSConfigJson>(VFSUtil.GetVFSConfigFilePath_InPackages(source_packages_root_folder), AssetLoadType.SystemIO, false);
                var config_mgr = new ConfigMiniManager(source_vfsconfig,
                    groupName => {
                        return Path.Combine(VFSUtil.GetMainPackage_AssetBundleManifests_Folder(source_packages_root_folder), groupName.GetMD5(true, true) + ".json");
                    });

                var target_vfsconfig = XConfig.GetJson<VFSConfigJson>(Path.Combine(target_data_root_path, VFSConst.Config_Runtime_FileName), AssetLoadType.SystemIO, false);
                var config_mgr_target = new ConfigMiniManager(target_vfsconfig,
                    groupName => {
                        return Path.Combine(target_data_root_path, VFSConst.MainPackage_AssetBundleManifests_Folder, groupName.GetMD5(true, true) + ".json");
                    });
                string ext_name_source = source_vfsconfig.AssetBundleFileExtension;
                if (!ext_name_source.StartsWith("."))
                    ext_name_source = "." + ext_name_source;
                ext_name_source = ext_name_source.ToLower();

                //string ext_name_target = target_vfsconfig.AssetBundleFileExtension;
                //if (!ext_name_target.StartsWith("."))
                //    ext_name_target = "." + ext_name_target;
                //ext_name_target = ext_name_target.ToLower();

                


                if (EnableEditorGUI)
                    EditorUtility.DisplayProgressBar("Patch Gen", IsChinese ? "正在对比Assets" : "Comparing assets", 0);

                int counter = 0;
                int counter_t = 0;
                int len = asset_hash_obj_source.Files.Length;

                //开始遍历逻辑
                foreach (var item in asset_hash_obj_source.Files)
                {
                    if (EnableEditorGUI)
                    {
                        counter++;
                        if(len > 100)
                        {
                            counter_t++;
                            if(counter_t > 50)
                            {
                                counter_t = 0;
                                EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                        }
                    }
                    //在组里查询一下
                    if (config_mgr.TryQueryAsset(item.p, out var group))
                    {
                        if (group.HandleMode == GroupHandleMode.LocalOrRemote || group.HandleMode == GroupHandleMode.RemoteOnly)
                            continue;

                        //查询target里面有没有这个资源
                        if(asset_hash_obj_target.TryGetFileHashValue(item.p,out string hash_target))
                        {
                            string asset_bundle_name = group.GetAssetBundleNameOfAsset(item.p) + ext_name_source;
                            //有，检查hash
                            if (!hash_target.Equals(item.h))
                            {
                                //不一致
                                
                                if (EnableLog)
                                    Debug.Log("    [Modify Asset]" + item.p);

                                if (!records.Modify_ReadWrite.Contains(asset_bundle_name))
                                    records.Modify_ReadWrite.Add(asset_bundle_name);

                            }
                            
                        }
                        else
                        {
                            //没有，是新增资源
                            string asset_bundle_name = group.GetAssetBundleNameOfAsset(item.p) + ext_name_source;
                            if (EnableLog)
                                Debug.Log("    [New Asset]" + item.p);

                            if (!records.Add_ReadWrite.Contains(asset_bundle_name))
                                records.Add_ReadWrite.Add(asset_bundle_name);
                        }

                    }
                }
                if (EnableEditorGUI)
                {
                    counter = 0;
                    counter_t = 0;
                    len = 0;
                    foreach(var group in config_mgr_target.mGroups)
                    {
                        if(group.FilesHash_VirtualDisk != null)
                        {
                            if (group.FilesHash_VirtualDisk.Files != null)
                                len += group.FilesHash_VirtualDisk.Files.Length;
                        }
                    }
                }
                //遍历target，寻找被删除的资源
                foreach(var group in config_mgr_target.mGroups)
                {
                    
                    if(group.FilesHash_VirtualDisk != null && group.FilesHash_VirtualDisk.Files != null)
                    {
                        foreach(var item in group.FilesHash_VirtualDisk.Files)
                        {
                            if (EnableEditorGUI)
                            {
                                counter++;
                                if (len > 100)
                                {
                                    counter_t++;
                                    if (counter_t > 50)
                                    {
                                        counter_t = 0;
                                        EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                                    }
                                }
                                else
                                {
                                    EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                                }
                            }
                            //找到我有的，但是current没有的
                            if (!config_mgr.TryFindAssetBundleName(item.p))
                            {
                                if (!records.Delete_ReadWrite.Contains(item.p))
                                    records.Delete_ReadWrite.Add(item.p);

                                if (EnableLog)
                                    Debug.Log("    [Delete AssetBundle]" + item.p);
                            }
                        }
                    }

                }

                if (EnableEditorGUI)
                    EditorUtility.ClearProgressBar();
            }
            else
            {
                var group_option_source = XConfig.GetJson<VFSGroupOption>(VFSUtil.GetExtensionPackages_GroupOptions_FilePath(source_packages_root_folder, branch.ExtensionGroupName), AssetLoadType.SystemIO, false);
                var group_option_target = XConfig.GetJson<VFSGroupOption>(Path.Combine(target_data_root_path, VFSConst.GetExtensionGroup_GroupOption_FileName), AssetLoadType.SystemIO, false);

                var group_source = new VFSEditorGroup(group_option_source);
                var group_target = new VFSEditorGroup(group_option_target);

                //ab hash
                group_source.SetVirtualDiskFileHash(XConfig.GetJson<FilesHashBook>(VFSUtil.GetExtensionGroup_AssetBundleHashFileFilePath(source_packages_root_folder, branch.ExtensionGroupName), AssetLoadType.SystemIO, false));
                group_target.SetVirtualDiskFileHash(XConfig.GetJson<FilesHashBook>(VFSEditorUtil.GetVersionData_AssetBundle_HashFile_FolderOrFilePath(true, branch.BranchName, target_record.versionCode), AssetLoadType.SystemIO, false));

                var ext_group_info_source = XConfig.GetJson<ExtensionGroupInfo>(VFSUtil.GetExtensionGroup_GroupInfo_Path_InGroupPath(VFSUtil.GetExtensionGroupFolder(source_packages_root_folder, branch.ExtensionGroupName)), AssetLoadType.SystemIO, false);
                string ab_ext_source = ext_group_info_source.AssetBundleExtension;
                if (!ab_ext_source.StartsWith("."))
                    ab_ext_source = "." + ab_ext_source;
                ab_ext_source = ab_ext_source.ToLower();

                if (EnableEditorGUI)
                    EditorUtility.DisplayProgressBar("Patch Gen", IsChinese ? "正在对比Assets" : "Comparing assets", 0);

                int counter = 0;
                int counter_t = 0;
                int len = asset_hash_obj_source.Files.Length;

                //开始遍历
                foreach (var item in asset_hash_obj_source.Files)
                {
                    if (EnableEditorGUI)
                    {
                        counter++;
                        if (len > 100)
                        {
                            counter_t++;
                            if (counter_t > 50)
                            {
                                counter_t = 0;
                                EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                        }
                    }
                    //查询
                    if (group_source.IsAssetPathMatch(item.p))
                    {
                        //检查target里有没有
                        if (asset_hash_obj_target.TryGetFileHashValue(item.p, out string hash_target)) 
                        {
                            //有，检查hash
                            if (!hash_target.Equals(item.h))
                            {
                                //不一致
                                string asset_bundle_name = group_source.GetAssetBundleNameOfAsset(item.p) + ab_ext_source;
                                if (EnableLog)
                                    Debug.Log("    [Modify Asset]" + item.p);

                                if (!records.Modify_ReadWrite.Contains(asset_bundle_name))
                                    records.Modify_ReadWrite.Add(asset_bundle_name);
                            }
                        }
                        else
                        {
                            //没有，是新增资源
                            string asset_bundle_name = group_source.GetAssetBundleNameOfAsset(item.p) + ab_ext_source;
                            if (EnableLog)
                                Debug.Log("    [New Asset]" + item.p);

                            if (!records.Add_ReadWrite.Contains(asset_bundle_name))
                                records.Add_ReadWrite.Add(asset_bundle_name);
                        }
                    }
                }

                if(group_target.FilesHash_VirtualDisk.Files != null)
                {
                    if (EnableEditorGUI)
                    {
                        counter = 0;
                        counter_t = 0;
                        len = group_target.FilesHash_VirtualDisk.Files.Length;
                    }
                    //遍历target，查找删除的资源
                    foreach (var item in group_target.FilesHash_VirtualDisk.Files)
                    {
                        if (EnableEditorGUI)
                        {
                            counter++;
                            if (len > 100)
                            {
                                counter_t++;
                                if (counter_t > 50)
                                {
                                    counter_t = 0;
                                    EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayProgressBar("Patch Gen", (IsChinese ? "正在对比Assets" : "Comparing assets") + $"{counter} / {len}", counter / len);
                            }
                        }

                        if (!group_source.FilesHash_VirtualDisk.TryGetFileHashValue(item.p,out _))
                        {
                            if (!records.Delete_ReadWrite.Contains(item.p))
                                records.Delete_ReadWrite.Add(item.p);

                            if (EnableLog)
                                Debug.Log("    [Delete AssetBundle]" + item.p);
                        }
                    }
                }
                

                if (EnableEditorGUI)
                    EditorUtility.ClearProgressBar();
            }

            PatchInfo patchInfo = new PatchInfo
            {
                patchCode = cur_version_code,
                platform = branch.Platform,
                targetVersionCode = target_record.versionCode
            };

            TinaX.IO.XDirectory.CreateIfNotExists(save_folder);
            string patch_temp_folder_path = Path.Combine(save_folder, "tmp_"+StringHelper.GetRandom(6));
            while (Directory.Exists(patch_temp_folder_path))
            {
                patch_temp_folder_path = Path.Combine(save_folder, "tmp_" + StringHelper.GetRandom(6));
            }
            Directory.CreateDirectory(patch_temp_folder_path);

            //保存文件
            string patch_info_save_path = Path.Combine(patch_temp_folder_path, VFSConst.Patch_Info_FileName);
            string patch_record_save_path = Path.Combine(patch_temp_folder_path, VFSConst.Patch_Record_FileName);
            XConfig.SaveJson(patchInfo, patch_info_save_path, AssetLoadType.SystemIO);
            records.SaveReady();
            XConfig.SaveJson(records, patch_record_save_path, AssetLoadType.SystemIO);
            string patch_assets_path = Path.Combine(patch_temp_folder_path, VFSConst.Patch_Assets_Folder_Name);
            Directory.CreateDirectory(patch_assets_path);
            //复制文件
            foreach(var item in records.Add_ReadWrite)
            {
                var assetbundle_path = VFSUtil.GetAssetBundlePathFromPackages(branch.BType != VersionBranch.BranchType.MainPackage,
                    source_packages_root_folder,
                    item,
                    branch.ExtensionGroupName);
                if(!File.Exists(assetbundle_path))
                {
                    Debug.LogError((IsChinese ? "制作补丁时发生错误，找不到文件：": "An error occurred while making the patch, the file could not be found:") + assetbundle_path);
                    continue;
                }
                string target_path = Path.Combine(patch_assets_path, item);
                TinaX.IO.XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                File.Copy(assetbundle_path, target_path);
            }
            foreach (var item in records.Modify_ReadWrite)
            {
                var assetbundle_path = VFSUtil.GetAssetBundlePathFromPackages(branch.BType != VersionBranch.BranchType.MainPackage,
                    source_packages_root_folder,
                    item,
                    branch.ExtensionGroupName);
                if (!File.Exists(assetbundle_path))
                {
                    Debug.LogError((IsChinese ? "制作补丁时发生错误，找不到文件：" : "An error occurred while making the patch, the file could not be found:") + assetbundle_path);
                    continue;
                }
                string target_path = Path.Combine(patch_assets_path, item);
                File.Copy(assetbundle_path, target_path);
            }

            //打包
            string[] files = Directory.GetFiles(patch_temp_folder_path);
            int total_len = files.Length;
            long zip_counter = 0;
            int zip_counter_t = 0;

            //打包
            
            ZipUtil.ZipDirectory(patch_temp_folder_path, save_file_name, fileName =>
            {
                if(EnableEditorGUI || EnableLog)
                {
                    zip_counter++;


                    if (EnableLog)
                    {
                        Debug.Log($"    [Make Patch]({zip_counter}/{total_len}):" + fileName);
                    }
                    if (EnableEditorGUI)
                    {

                        if (total_len > 100)
                        {
                            zip_counter_t++;
                            if (zip_counter_t >= 20)
                            {
                                zip_counter_t = 0;

                                if (EnableEditorGUI) EditorUtility.DisplayProgressBar("Create Zip", $"{zip_counter}/{total_len}\n{fileName}", zip_counter / total_len);
                            }
                        }
                        else
                        {
                            if (EnableEditorGUI) EditorUtility.DisplayProgressBar("Create Zip", $"{zip_counter}/{total_len}\n{fileName}", zip_counter / total_len);
                        }


                    }
                }
                
                
            });

            if (EnableEditorGUI)
                EditorUtility.ClearProgressBar();

            Directory.Delete(patch_temp_folder_path, true);  //Debug临时注释掉
            Application.OpenURL(new Uri(save_folder).ToString());
        }

        private bool? _isChinese;
        private bool IsChinese
        {
            get
            {
                if (_isChinese == null)
                {
                    _isChinese = (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified);
                }
                return _isChinese.Value;
            }
        }

        class ConfigMiniManager
        {
            private VFSConfigJson mConfig;
            public List<VFSEditorGroup> mGroups;
            public ConfigMiniManager(VFSConfigJson config, Func<string,string> get_assetbundle_hash_callback)
            {
                mConfig = config;
                mGroups = new List<VFSEditorGroup>();
                foreach(var group_option in mConfig.Groups)
                {
                    if (group_option.GroupAssetsHandleMode == GroupHandleMode.LocalOrRemote || group_option.GroupAssetsHandleMode == GroupHandleMode.RemoteOnly)
                        continue;
                    if (group_option.ExtensionGroup)
                        continue;
                    mGroups.Add(new VFSEditorGroup(group_option));
                }

                //加载出assetbundle_hash
                foreach(var group in mGroups)
                {
                    string path = get_assetbundle_hash_callback(group.GroupName);
                    group.SetVirtualDiskFileHash(XConfig.GetJson<FilesHashBook>(path, AssetLoadType.SystemIO, false));
                }
            }

            public bool TryQueryAsset(string asset_path, out VFSEditorGroup group)
            {
                foreach(var _group in mGroups)
                {
                    if (_group.IsAssetPathMatch(asset_path))
                    {
                        group = _group;
                        return true;
                    }
                }
                group = null;
                return false;
            }
            
            public bool TryFindAssetBundleName(string assetBundleName)
            {
                foreach(var group  in mGroups)
                {
                    if(group.FilesHash_VirtualDisk != null)
                    {
                        return group.FilesHash_VirtualDisk.TryGetFileHashValue(assetBundleName, out _);
                    }
                }
                return false;
            }

        }

    }
}
