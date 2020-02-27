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
        private string mVersionRootFolderPath = VFSEditorConst.PROJECT_VFS_FILES_ROOT_FOLDER_PATH;  //根目录
        private string mVersionDataFolderPath = VFSEditorConst.VFS_VERSION_RECORD_Data_FOLDER_PATH; //Data目录
        private string mVersionData_BranchIndex_FolderPath;

        private string mVersionMainFilePath = VFSEditorConst.VFS_VERSION_RECORD_FILE_PATH; //主文件

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

            if(mVersionMainData.branches == null || mVersionMainData.branches.Length == 0)
            {
                mVersionMainData.branches = new string[] { "master" };
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

                    XConfig.SaveJson(obj, branchIndexFilePath, AssetLoadType.SystemIO);
                }
            }

            #endregion
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
                        
