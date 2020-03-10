using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEditor;
using TinaX;
using TinaX.Utils;
using TinaX.IO;
using TinaX.VFSKit.Const;
using TinaX.VFSKitInternal;
using TinaXEditor.VFSKit;
using TinaXEditor.VFSKit.Utils;
using TinaXEditor.VFSKit.Const;
using TinaX.VFSKitInternal.Utils;

namespace TinaXEditor.VFSKit.Versions
{
    public class VFSVersionsManagerEditor
    {
        private readonly string mVersionRootFolderPath = VFSEditorConst.VFS_VERSION_ROOT_FOLDER_PATH;  //根目录
        private readonly string mVersionDataFolderPath = VFSEditorConst.VFS_VERSION_RECORD_Data_FOLDER_PATH; //Data目录
        private readonly string mVersionData_BranchIndex_FolderPath;
        //private readonly string mVersionData_Branches_Data_FolderPath; //这个目录下接着分支名目录，分支名目录是版本号目录，版本号目录下发存放每次打包出来的具体文件记录
        private readonly string mVersionBinaryFolderPath = VFSEditorConst.VFS_VERSION_RECORD_Binary_FOLDER_PATH;
        private readonly string mVersionBinary_Branches_FolderPath; //目录下面也是分支名-> 版本号，里面存放打包后的资源文件

        private readonly string mVersionMainFilePath = VFSEditorConst.VFS_VERSION_RECORD_FILE_PATH; //主文件

        //private const string DefaultBranchName = "master";

        private VersionsModel mVersionMainData;
        private Dictionary<string, VersionBranch> mDict_Branches = new Dictionary<string, VersionBranch>(); //string: branch name

        public VFSVersionsManagerEditor()
        {
            mVersionData_BranchIndex_FolderPath = Path.Combine(mVersionDataFolderPath, "BrancheIndexes");
            //mVersionData_Branches_Data_FolderPath = Path.Combine(mVersionDataFolderPath, "Branches");
            mVersionBinary_Branches_FolderPath = Path.Combine(mVersionBinaryFolderPath, "Branches");
            XDirectory.CreateIfNotExists(mVersionRootFolderPath);
            XDirectory.CreateIfNotExists(mVersionDataFolderPath);
            XDirectory.CreateIfNotExists(mVersionData_BranchIndex_FolderPath);

            #region 初始化版本数据

            //主索引文件
            bool flag_create = false;
            if (File.Exists(mVersionMainFilePath))
            {
                try
                {
                    mVersionMainData = XConfig.GetJson<VersionsModel>(mVersionMainFilePath, AssetLoadType.SystemIO, false);
                }
                catch
                {
                    flag_create = true;
                }
            }
            else
                flag_create = true;

            if (flag_create)
            {
                mVersionMainData = new VersionsModel();
            }

            if(mVersionMainData.branches == null)
            {
                mVersionMainData.branches = new string[0];
            }

            //各个分支的配置文件
            foreach(var branchName in mVersionMainData.branches)
            {
                string branchIndexFilePath = Path.Combine(mVersionData_BranchIndex_FolderPath, $"{branchName}.json");
                bool flag_create_branch = false;

                if (File.Exists(branchIndexFilePath))
                {
                    try
                    {
                        var obj = XConfig.GetJson<VersionBranch>(branchIndexFilePath, AssetLoadType.SystemIO, false);
                        if(obj != null)
                        {
                            if (mDict_Branches.ContainsKey(branchName))
                            {
                                mDict_Branches[branchName] = obj;
                            }
                            else
                            {
                                mDict_Branches.Add(branchName, obj);
                            }
                        }
                    }
                    catch
                    {
                        flag_create_branch = true;
                    }


                }
                else
                    flag_create_branch = true;

                if (flag_create_branch)
                {
                    var obj = new VersionBranch();
                    obj.BranchName = branchName;
                    obj.BType = VersionBranch.BranchType.MainPackage;

                    mDict_Branches.Add(branchName, obj);
                    SaveBranchFile(ref obj);
                }
            }

            #endregion
        }


        public string[] GetBranchNames()
        {
            return mVersionMainData.Branches_ReadWrite.ToArray();
        }

        public bool IsBranchExists(string branchName)
        {
            string blower = branchName.ToLower();
            return mVersionMainData.Branches_ReadWrite.Any(b => b.ToLower() == blower);
        }

        public bool AddBranch(string branchName, VersionBranch.BranchType type, XRuntimePlatform platform, string desc = null, string extGroupName = null)
        {
            if (!branchName.IsValidFileName()) return false;
            if (this.IsBranchExists(branchName)) return false;
            if (type == VersionBranch.BranchType.ExtensionGroup && extGroupName.IsNullOrEmpty()) return false;
            mVersionMainData.Branches_ReadWrite.Add(branchName);
            var branch = new VersionBranch();
            branch.BranchName = branchName;
            branch.BType = type;
            branch.Desc = desc;
            branch.ExtensionGroupName = extGroupName;
            branch.Platform = platform;

            if (mDict_Branches.ContainsKey(branchName))
            {
                mDict_Branches[branchName] = branch;
            }
            else
            {
                mDict_Branches.Add(branchName, branch);
            }
            SaveVersionMainData(ref mVersionMainData, mVersionMainFilePath);
            SaveBranchFile(ref branch);
            return true;
        }

        public void RemoveBranch(string branchName)
        {
            string blower = branchName.ToLower();
            for(int i = mVersionMainData.Branches_ReadWrite.Count -1; i >= 0; i--)
            {
                if(mVersionMainData.Branches_ReadWrite[i].ToLower() == blower)
                {
                    string branch = mVersionMainData.Branches_ReadWrite[i];

                    //删除branch的目录信息

                    //index
                    string branch_index_path = Path.Combine(mVersionData_BranchIndex_FolderPath, branch + ".json");
                    XFile.DeleteIfExists(branch_index_path);
                    //版本数据
                    string branch_version_data_folder_path = VFSEditorUtil.GetVersionDataRootFolderPath_InProjectVersion(ref branchName);
                    XDirectory.DeleteIfExists(branch_version_data_folder_path, true);

                    //二进制文件
                    string branch_binary_data_folder_path = Path.Combine(mVersionBinary_Branches_FolderPath, branch);
                    XDirectory.DeleteIfExists(branch_binary_data_folder_path, true);

                    //删除索引记录
                    mVersionMainData.Branches_ReadWrite.RemoveAt(i);
                    //删除字典
                    if (mDict_Branches.ContainsKey(branch))
                        mDict_Branches.Remove(branch);

                }
            }

            //保存更新后的分子索引
            SaveVersionMainData(ref mVersionMainData, mVersionMainFilePath);
        }

        public long GetMaxVersion(string branchName,out string versionName, out string versionDesc)
        {
            if (mDict_Branches.ContainsKey(branchName))
            {
                var vr = mDict_Branches[branchName].GetMaxVersion();
                if(vr != null)
                {
                    versionName = vr.Value.versionName;
                    versionDesc = vr.Value.desc;
                    return vr.Value.versionCode;
                }
                else
                {
                    versionName = string.Empty;
                    versionDesc = string.Empty;
                    return -1;
                }
            }
            else
            {
                versionName = string.Empty;
                versionDesc = string.Empty;
                return -1;
            }
        }

        public long GetMaxVersion(string branchName) => this.GetMaxVersion(branchName, out _, out _);
        public long GetMaxVersion(string branchName, out string versionName) => this.GetMaxVersion(branchName, out versionName, out _);

        public bool TryGetVersionBranch(string name, out VersionBranch branch)
        {
            return mDict_Branches.TryGetValue(name, out branch);
        }

        public string[] GetBranchNamesByMainPackage(XRuntimePlatform platform)
        {
            List<string> result = new List<string>();
            foreach(var item in mDict_Branches)
            {
                if(item.Value.BType == VersionBranch.BranchType.MainPackage && item.Value.Platform == platform)
                {
                    result.Add(item.Key);
                }
            }
            return result.ToArray();
        }

        public string[] GetBranchNamesByExtensionGroup(XRuntimePlatform platform,string extensionGroupName)
        {
            List<string> result = new List<string>();
            foreach (var item in mDict_Branches)
            {
                if (item.Value.BType == VersionBranch.BranchType.ExtensionGroup && item.Value.Platform == platform && item.Value.ExtensionGroupName == extensionGroupName)
                {
                    result.Add(item.Key);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 获取用于某个扩展组的所有分支名称
        /// </summary>
        /// <param name="extensionGroupName"></param>
        /// <returns></returns>
        public string[] GetBranchNamesByExtensionGroup(string extensionGroupName)
        {
            List<string> result = new List<string>();
            foreach (var item in mDict_Branches)
            {
                if (item.Value.BType == VersionBranch.BranchType.ExtensionGroup && item.Value.ExtensionGroupName == extensionGroupName)
                {
                    result.Add(item.Key);
                }
            }
            return result.ToArray();
        }

        public VersionRecord? GetMaxVersionRecord(string branchName)
        {
            if (mDict_Branches.ContainsKey(branchName))
            {
                return mDict_Branches[branchName].GetMaxVersion();
                
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 添加版本记录
        /// </summary>
        /// <param name=""></param>
        public void AddVersionRecord(string branchName, long versionCode , string versionName ,string versionDesc,bool saveBinary,bool dialog = true, bool log = true)
        {
            //编辑器那边限制了不能添加“比最大版本号更小的版本号”的版本，（也就是说版本号只能变大），但是这里实际上没做这个限制。以后如果有需要，可以让编辑器UI上去掉限制。
            if(mDict_Branches.TryGetValue(branchName,out var branch))
            {
                //判断一下版本号啦
                if(versionCode >= 0 && !branch.IsVersionCodeExists(versionCode))
                {

                    bool isMainPackage = (branch.BType == VersionBranch.BranchType.MainPackage);
                    bool flag_process_error = false; //处理文件过程中如果出错则中断操作且不记录数据

                    string platform_name = XPlatformUtil.GetNameText(branch.Platform);
                    string source_packages_folder_path = VFSEditorUtil.GetSourcePackagesFolderPath(ref platform_name);
                    string data_folder = VFSEditorUtil.GetVersionDataFolderPath_InProjectVersion(ref branch.BranchName, ref versionCode); //存放数据的地方


                    try
                    {
                        XDirectory.DeleteIfExists(data_folder, true);
                        Directory.CreateDirectory(data_folder);

                        //复制并存档assets_hash文件
                        string assets_hash_path = isMainPackage ? VFSEditorUtil.GetMainPackage_AssetsHashFilePath_InSourcePackagesFolder(ref platform_name) : VFSEditorUtil.GetExtensionGroup_AssetsHashFilePath_InSourcePackagesFolder(ref platform_name, ref branch.ExtensionGroupName);
                        string assets_hash_target_path = Path.Combine(data_folder, VFSConst.AssetsHashFileName);
                        if (File.Exists(assets_hash_path))
                        {
                            File.Copy(assets_hash_path, assets_hash_target_path, true);
                        }

                        //复制并存档Manifest文件
                        string manifest_target_path = VFSEditorUtil.GetVersionData_Manifest_FolderOrFilePath(!isMainPackage, branchName, versionCode);
                        string manifest_path = isMainPackage ? VFSEditorUtil.GetMainPackage_AssetBundleManifestsFolderPath_InSourcePackagesFolder(platform_name) : VFSEditorUtil.GetExtensionGroup_AssetBundleManifestPath_InInSourcePackagesFolder(platform_name, branch.ExtensionGroupName);
                        if (isMainPackage)
                        {
                            if (Directory.Exists(manifest_path))
                                XDirectory.CopyDir(manifest_path, manifest_target_path);
                        }
                        else
                        {
                            if (File.Exists(manifest_path))
                                File.Copy(manifest_path, manifest_target_path);
                        }

                        //复制并存档AssetBundleHashs
                        string ab_hash_path = isMainPackage ? VFSEditorUtil.GetMainPackage_AssetBundle_HashFiles_FolderPath_InSourcePackagesFolder(platform_name) : VFSEditorUtil.GetExtensionGroup_AssetBundle_HashFiles_Path_InInSourcePackagesFolder(platform_name, branch.ExtensionGroupName);
                        string ab_hash_target_path = VFSEditorUtil.GetVersionData_AssetBundle_HashFile_FolderOrFilePath(!isMainPackage, branchName, versionCode);
                        if (isMainPackage)
                        {
                            if (Directory.Exists(ab_hash_path))
                                XDirectory.CopyDir(ab_hash_path, ab_hash_target_path);
                        }
                        else
                        {
                            if (File.Exists(ab_hash_path))
                                File.Copy(ab_hash_path, ab_hash_target_path);
                        }

                        //复制并存档editor build info
                        string editor_build_info_path = VFSEditorUtil.Get_EditorBuildInfoPath(VFSEditorUtil.GetSourcePackagesFolderPath(ref platform_name));
                        if (File.Exists(editor_build_info_path))
                        {
                            string target_path = VFSEditorUtil.GetVersionData_EditorBuildInfo_Path(branchName, versionCode);
                            File.Copy(editor_build_info_path, target_path);
                        }

                        //复制并存档 build info
                        string build_info_path = VFSUtil.GetMainPackage_BuildInfo_Path(VFSEditorUtil.GetSourcePackagesFolderPath(ref platform_name));
                        if (File.Exists(build_info_path))
                        {
                            //存档
                            string target_path = VFSEditorUtil.GetVersionData_BuildInfo_Path(branchName, versionCode);
                            File.Copy(build_info_path, target_path);

                            //反写版本信息到source package
                            string build_info_json = File.ReadAllText(target_path, Encoding.UTF8);
                            var obj = JsonUtility.FromJson<BuildInfo>(build_info_json);

                            //写出版本信息
                            var version_info = new PackageVersionInfo
                            {
                                version = versionCode,
                                versionName = versionName,
                                buildId = obj.BuildID
                            };
                            string version_info_path = isMainPackage ? VFSEditorUtil.Get_MainPackage_PackageVersionFilePath_InSourcePackages(ref platform_name) : VFSEditorUtil.Get_ExtensionGroups_PackageVersionFilePath_InSourcePackages(ref platform_name, ref branch.ExtensionGroupName);
                            XFile.DeleteIfExists(version_info_path);
                            XConfig.SaveJson(version_info, version_info_path, AssetLoadType.SystemIO);

                            //检查当前StreamingAssets中是否有与之build id一致的情况，如果有，也写出
                            if (isMainPackage)
                            {
                                string buildinfo_in_stream = VFSUtil.GetMainPackage_BuildInfo_Path(VFSUtil.GetPackagesRootFolderInStreamingAssets(platform_name));
                                if (File.Exists(buildinfo_in_stream))
                                {
                                    try
                                    {
                                        var obj_stream = XConfig.GetJson<BuildInfo>(buildinfo_in_stream, AssetLoadType.SystemIO, false);
                                        if(obj_stream.BuildID == obj.BuildID)
                                        {
                                            //一致，写出
                                            string target_stream = VFSUtil.GetMainPackage_VersionInfo_Path(VFSUtil.GetPackagesRootFolderInStreamingAssets(platform_name));
                                            XConfig.SaveJson(version_info, target_stream, AssetLoadType.SystemIO);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            else
                            {
                                string buildinfo_in_stream = VFSUtil.GetExtensionGroup_BuildInfo_Path(VFSUtil.GetPackagesRootFolderInStreamingAssets(platform_name), branch.ExtensionGroupName);
                                if (File.Exists(buildinfo_in_stream))
                                {
                                    try
                                    {
                                        var obj_stream = XConfig.GetJson<BuildInfo>(buildinfo_in_stream, AssetLoadType.SystemIO, false);
                                        if (obj_stream.BuildID == obj.BuildID)
                                        {
                                            //一致，写出
                                            string target_stream = VFSUtil.GetExtensionGroup_VersionInfo_Path(VFSUtil.GetPackagesRootFolderInStreamingAssets(platform_name), branch.ExtensionGroupName);
                                            XConfig.SaveJson(version_info, target_stream, AssetLoadType.SystemIO);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }

                    }
                    catch(Exception e)
                    {
                        XDirectory.DeleteIfExists(data_folder, true);
                        flag_process_error = true;
                        throw e;
                    }

                    //保存二进制文件
                    if(saveBinary && !flag_process_error)
                    {
                        string binary_path = VFSEditorUtil.Get_AssetsBinaryFolderPath_InVersion(ref branchName, ref versionCode);

                        try
                        {
                            long total_count = 0;
                            //把所有二进制文件直接全拷进去
                            string binary_path_temp = Path.Combine(binary_path, "temp");
                            XDirectory.DeleteIfExists(binary_path_temp, true);
                            Directory.CreateDirectory(binary_path_temp);
                            /*
                             * 首先，先把vfs_root和vfs_remote的文件得都还原到temp目录下，
                             * 然后，把temp目录里的东西整体打包成zip，存储在binary_path
                             */

                            //移动文件
                            if (isMainPackage)
                            {
                                string local_path = VFSEditorUtil.Get_MainPackage_LocalAssetsFolderPath_InSourcePackages(ref platform_name);
                                int local_path_len = local_path.Length + 1;
                                string[] local_files = Directory.GetFiles(local_path, "*.*", SearchOption.AllDirectories);
                                if (local_files != null && local_files.Length > 0)
                                {
                                    ArrayUtil.RemoveDuplicationElements(ref local_files);
                                    foreach (var item in local_files)
                                    {
                                        total_count++;
                                        string pure_path = item.Substring(local_path_len, item.Length - local_path_len);
                                        string target_path = Path.Combine(binary_path_temp, pure_path);
                                        XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                                        File.Copy(item, target_path);
                                    }
                                }

                                string remote_path = VFSEditorUtil.Get_MainPackage_RemoteAssetsFolderPath_InSourcePackages(ref platform_name);
                                int remote_path_len = remote_path.Length + 1;
                                string[] remote_files = Directory.GetFiles(remote_path, "*.*", SearchOption.AllDirectories);
                                if (remote_files != null && remote_files.Length > 0)
                                {
                                    total_count++;
                                    ArrayUtil.RemoveDuplicationElements(ref remote_files);
                                    foreach (var item in remote_files)
                                    {
                                        string pure_path = item.Substring(remote_path_len, item.Length - remote_path_len);
                                        string target_path = Path.Combine(binary_path_temp, pure_path);
                                        XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                                        File.Copy(item, target_path);
                                    }
                                }

                            }
                            else
                            {
                                string group_path = VFSEditorUtil.Get_ExtensionGroupFolderPath_InSourcePackages(ref platform_name, ref branch.ExtensionGroupName);
                                int group_path_len = group_path.Length + 1;
                                string[] group_files = Directory.GetFiles(group_path, "*.*", SearchOption.AllDirectories);
                                if (group_files != null && group_files.Length > 0)
                                {
                                    total_count++;
                                    ArrayUtil.RemoveDuplicationElements(ref group_files);
                                    foreach (var item in group_files)
                                    {
                                        string pure_path = item.Substring(group_path_len, item.Length - group_path_len);
                                        string target_path = Path.Combine(binary_path_temp, pure_path);
                                        XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                                        File.Copy(item, target_path);
                                    }
                                }

                            }

                            long zip_counter = 0;
                            int zip_counter_t = 0;

                            //打包
                            string zip_file_path = Path.Combine(binary_path, VFSEditorConst.VFS_VERSION_AssetsBinary_Zip_Name);
                            ZipUtil.ZipDirectory(binary_path_temp, zip_file_path, fileName =>
                            {
                                if (log || dialog)
                                {
                                    zip_counter++;
                                    if (total_count > 100)
                                    {
                                        zip_counter_t++;
                                        if (zip_counter_t >= 20)
                                        {
                                            zip_counter_t = 0;
                                            if (log) Debug.Log($"    Create Zip: {zip_counter}/{total_count}");
                                            if (dialog) EditorUtility.DisplayProgressBar("Create Zip", $"{zip_counter}/{total_count}\n{fileName}", zip_counter / total_count);
                                        }
                                    }
                                    else
                                    {
                                        if (log) Debug.Log($"    Create Zip: {zip_counter}/{total_count} : {fileName}");
                                        if (dialog) EditorUtility.DisplayProgressBar("Create Zip", $"{zip_counter}/{total_count}\n{fileName}", zip_counter / total_count);
                                    }
                                }
                            });

                            if (dialog)
                                EditorUtility.ClearProgressBar(); //上面这个应该是同步方法，不会有时间错乱。（吧


                            //删除temp
                            XDirectory.DeleteIfExists(binary_path_temp);
                        }
                        catch(Exception e)
                        {
                            flag_process_error = true;
                            XDirectory.DeleteIfExists(binary_path);
                            throw e;
                        }
                    }

                    if (!flag_process_error)
                    {
                        //登记到索引
                        var vr = new VersionRecord()
                        {
                            versionCode = versionCode,
                            versionName = versionName,
                            desc = versionDesc
                        };
                        //记录版本
                        branch.AddVersion(ref vr);

                        //保存版本索引
                        SaveBranchFile(ref branch);
                    }
                }
            }
        }

        public void RemoveProfileRecord(ref VersionBranch branch, long versionCode)
        {
            bool isMainPackage = (branch.BType == VersionBranch.BranchType.MainPackage);

            //删除data
            string data_folder = VFSEditorUtil.GetVersionDataFolderPath_InProjectVersion(ref branch.BranchName, ref versionCode); //存放数据的地方
            XDirectory.DeleteIfExists(data_folder, true);

            //删除二进制
            string binary_folder = VFSEditorUtil.Get_AssetsBinaryFolderPath_InVersion(ref branch.BranchName, ref versionCode);
            XDirectory.DeleteIfExists(binary_folder);

            //在索引中删除记录
            foreach(var item in branch.VersionRecords_ReadWrite)
            {
                if(item.versionCode == versionCode)
                {
                    branch.VersionRecords_ReadWrite.Remove(item);
                    break;
                }
            }
            //保存索引
            SaveBranchFile(ref branch);
        }

        private void SaveVersionMainData(ref VersionsModel data , string path)
        {
            data.ReadySave();
            XConfig.SaveJson(data, path, AssetLoadType.SystemIO);
        }

        public void SaveBranchFile(ref VersionBranch data)
        {
            string branch_path = Path.Combine(mVersionData_BranchIndex_FolderPath, data.BranchName + ".json");
            data.ReadySave();
            XConfig.SaveJson(data, branch_path, AssetLoadType.SystemIO);
        }


        private void createDefaultIfBranchNotExists(string branchName, VersionBranch.BranchType branchType)
        {
            if (!mDict_Branches.ContainsKey(branchName))
            {
                mDict_Branches.Add(branchName, new VersionBranch()
                {
                    BranchName = branchName,
                    BType = branchType,
                });
            }
        }


    }
}



// Editor下的目录结构
//- TinaX*
//    - VFS_Version*
//        - Data*
//            - VFSVersion.json
//            - BrancheIndexes
//                - %branchName%.json
//                - ...
//            - Branches*
//                - %branchName%*
//                    - %versionCode%*
//                        - * asset_data*
//        - Binary*
//            - Branches*
//                - %branchName%
//                    - %versionCode%
//                    - ...
//                - ...
                        
