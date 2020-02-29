using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TinaX;
using TinaX.Utils;
using TinaX.IO;
using TinaXEditor.VFSKit;
using TinaXEditor.VFSKit.Const;

namespace TinaXEditor.VFSKit.Versions
{
    public class VFSVersionsManagerEditor
    {
        private readonly string mVersionRootFolderPath = VFSEditorConst.VFS_VERSION_RECORD_ROOT_FOLDER_PATH;  //根目录
        private readonly string mVersionDataFolderPath = VFSEditorConst.VFS_VERSION_RECORD_Data_FOLDER_PATH; //Data目录
        private readonly string mVersionData_BranchIndex_FolderPath;
        private readonly string mVersionData_Branches_Data_FolderPath; //这个目录下接着分支名目录，分支名目录是版本号目录，版本号目录下发存放每次打包出来的具体文件记录
        private readonly string mVersionBinaryFolderPath = VFSEditorConst.VFS_VERSION_RECORD_Binary_FOLDER_PATH;
        private readonly string mVersionBinary_Branches_FolderPath; //目录下面也是分支名-> 版本号，里面存放打包后的资源文件

        private readonly string mVersionMainFilePath = VFSEditorConst.VFS_VERSION_RECORD_FILE_PATH; //主文件

        //private const string DefaultBranchName = "master";

        private VersionsModel mVersionMainData;
        private Dictionary<string, VersionBranch> mDict_Branches = new Dictionary<string, VersionBranch>(); //string: branch name

        public VFSVersionsManagerEditor()
        {
            mVersionData_BranchIndex_FolderPath = Path.Combine(mVersionDataFolderPath, "BrancheIndexes");
            mVersionData_Branches_Data_FolderPath = Path.Combine(mVersionDataFolderPath, "Branches");
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
                    //版本数据
                    string branch_version_data_folder_path = Path.Combine(mVersionData_Branches_Data_FolderPath, branch);
                    XDirectory.DeleteIfExists(branch_version_data_folder_path, true);

                    //二进制文件
                    string branch_binary_data_folder_path = Path.Combine(mVersionBinary_Branches_FolderPath, branch);
                    XDirectory.DeleteIfExists(branch_version_data_folder_path, true);

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
        public void AddVersionRecord(string branchName, long versionCode , string versionName ,string versionDesc,bool saveBinary)
        {
            //编辑器那边限制了不能添加“比最大版本号更小的版本号”的版本，（也就是说版本号只能变大），但是这里实际上没做这个限制。以后如果有需要，可以让编辑器UI上去掉限制。
            if(mDict_Branches.TryGetValue(branchName,out var branch))
            {
                //判断一下版本号啦
                if(versionCode >= 0 && !branch.IsVersionCodeExists(versionCode))
                {
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

                    //记录数据
                    string platform_name = XPlatformUtil.GetNameText(branch.Platform);
                    string source_packages_folder_path = Path.Combine(VFSEditorConst.PROJECT_VFS_FILES_ROOT_FOLDER_PATH, platform_name);
                    string data_folder = Path.Combine(mVersionData_Branches_Data_FolderPath, branch.BranchName, versionCode.ToString()); //存放数据的地方
                    XDirectory.CreateIfNotExists(data_folder);
                    if (branch.BType == VersionBranch.BranchType.MainPackage)
                    {
                        //
                    }
                }
            }
        }

        private void SaveVersionMainData(ref VersionsModel data , string path)
        {
            data.ReadySave();
            XConfig.SaveJson(data, path, AssetLoadType.SystemIO);
        }

        private void SaveBranchFile(ref VersionBranch data)
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
//            - Brances*
//                - %branchName%
//                    - %versionCode%
//                    - ...
//                - ...
                        
