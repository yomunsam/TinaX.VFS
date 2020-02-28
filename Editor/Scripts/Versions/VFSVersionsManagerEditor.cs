using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TinaX;
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

        private readonly string mVersionMainFilePath = VFSEditorConst.VFS_VERSION_RECORD_FILE_PATH; //主文件

        //private const string DefaultBranchName = "master";

        private VersionsModel mVersionMainData;
        private Dictionary<string, VersionBranch> mDict_Branches = new Dictionary<string, VersionBranch>(); //string: branch name

        public VFSVersionsManagerEditor()
        {
            mVersionData_BranchIndex_FolderPath = Path.Combine(mVersionDataFolderPath, "BrancheIndexes");
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
                        
