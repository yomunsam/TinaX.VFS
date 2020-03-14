using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TinaX;
using TinaX.Utils;
using UnityEngine;
using UnityEditor;
using TinaXEditor.VFSKit.Utils;
using TinaXEditor.VFSKit.Const;
using TinaX.VFSKitInternal.Utils;


namespace TinaXEditor.VFSKit.Versions
{
    class CreateVersionRecordGUI : EditorWindow
    {
        #region params
        public static XRuntimePlatform? VFS_Platform { get; set; }
        public static string BranchName { get; set; }
        #endregion

        static CreateVersionRecordGUI wnd;

        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<CreateVersionRecordGUI>();
                wnd.titleContent = new GUIContent("VFS Archiver");
                wnd.minSize = new Vector2(399, 400);
                //wnd.maxSize = new Vector2(400, 500);
                var rect = wnd.position;
                rect.width = 399;
                rect.height = 410;
                wnd.position = rect;
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }
        private static bool? _isChinese;
        private static bool IsChinese
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

        private bool flag_platform_from_param = false; //从上面的静态变量里接收到了参数
        private XRuntimePlatform platform_from_param;
        private List<string> match_branches_from_param = new List<string>(); //根据参数计算出的符合给定的平台的分支参数
        private string[] match_branches_from_param_arr; //根据参数计算出的符合给定的平台的分支参数

        private bool flag_branchName_from_param = false;
        private string branchName_from_param;
        private bool isBrancNameFromParamValid = false ; //从静态变量里接收到的分支参数是否有效

        private VersionBranch mCurBranch;
        private string mCurSelectBranchName;

        private string[] mAllBranchNames;
        private int mCur_Select_Branch_Index;
        private bool mAllBranch_Inited = false; //获取过一次数据之后变成true

        private bool IsSourcePackagesValidByCurBranch = false; // 根据当前选择的分支，是否有对应的有效的打包文件
        private string IsSourcePackagesValidByCurBranch_BranchName; //上面这个数据是由哪个分支计算出来的

        private long curMaxVersionCode;    //所选分支的最大版本号
        private string curMaxVersionCode_BranchName; //上面这个数据是由哪个分支计算出来的

        private bool mBranchAndPlatformValid = false; //这个数据在OnGUI里每帧刷新一次，如果分支或者平台相关的设定无效。这里就置为false;

        private long mCurVersionCode;
        private string mCurVersionName;
        private string mCurVersionDecs;

        private bool mCurSaveBinary; //是否保存二进制文件

        private void OnEnable()
        {
            if(VFS_Platform != null)
            {
                flag_platform_from_param = true;
                platform_from_param = VFS_Platform.Value;

                //如果参数给的是平台，那么要倒推出，当前该平台可用的分支
                string[] main_package_branches = VFSManagerEditor.VersionManager.GetBranchNamesByMainPackage(platform_from_param);
                if (main_package_branches != null && main_package_branches.Length > 0)
                    match_branches_from_param.AddRange(main_package_branches);
                string platform_name = XPlatformUtil.GetNameText(platform_from_param);
                string source_packages_folder_path = Path.Combine(VFSEditorConst.PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH, platform_name);
                string[] extension_groups_in_source_package_folder = VFSUtil.GetValidExtensionGroupNames(VFSUtil.GetExtensionPackageRootFolderInPackages(source_packages_folder_path));
                foreach(var groupName in extension_groups_in_source_package_folder)
                {
                    string[] group_branches = VFSManagerEditor.VersionManager.GetBranchNamesByExtensionGroup(platform_from_param, groupName);
                    if(group_branches != null && group_branches.Length > 0)
                    {
                        match_branches_from_param.AddRange(group_branches);
                    }
                }
                match_branches_from_param_arr = match_branches_from_param.ToArray();
            }

            if (!BranchName.IsNullOrEmpty())
            {
                flag_branchName_from_param = true;
                branchName_from_param = BranchName;
                isBrancNameFromParamValid = VFSManagerEditor.VersionManager.TryGetVersionBranch(branchName_from_param, out mCurBranch);
                mCurSelectBranchName = BranchName;
            }
        }

        /// <summary>
        /// 准备UI数据
        /// </summary>
        private void ReadyGUIData()
        {
            
        }

        private void OnGUI()
        {
            //GUILayout.Label("width " + this.position.width + "  height:" + this.position.height);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);
            mBranchAndPlatformValid = true;
            #region 版本分支
            if (flag_branchName_from_param)
            {
                //参数指定了分支名
                if (!isBrancNameFromParamValid)
                {
                    //给出的分支名无效
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(IsChinese ? $"出错：指定的分支名 {branchName_from_param} 无效" : $"Error: invalid branch name {branchName_from_param} specified");
                    GUILayout.FlexibleSpace();
                    mBranchAndPlatformValid = false;
                }
                else
                {
                    //使用给定的分支信息继续
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(IsChinese ? "版本分支：" : "Version Branch: ", GUILayout.Width(110));
                    EditorGUILayout.LabelField(branchName_from_param);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                //手动选择分支
                if (flag_platform_from_param)
                {
                    //从参数里获取到了平台信息，那么就要根据指定的平台来选择分支
                    if(match_branches_from_param.Count == 0)
                    {
                        mBranchAndPlatformValid = false;
                        GUILayout.Label(IsChinese ? $"没有平台{platform_from_param.ToString()}对应的分支信息" : $"There is no branch info corresponding to platform {platform_from_param.ToString()}");
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(IsChinese ? "版本分支：" : "Version Branch: ", GUILayout.Width(110));
                        mCur_Select_Branch_Index = EditorGUILayout.Popup(mCur_Select_Branch_Index, match_branches_from_param_arr);
                        EditorGUILayout.EndHorizontal();
                        mCurSelectBranchName = match_branches_from_param_arr[mCur_Select_Branch_Index];
                    }
                }
                else
                {
                    if (!mAllBranch_Inited)
                    {
                        mAllBranchNames = VFSManagerEditor.VersionManager.GetBranchNames();
                        mCur_Select_Branch_Index = 0;
                        mAllBranch_Inited = true;
                    }
                    if (mAllBranchNames.Length == 0)
                    {
                        mBranchAndPlatformValid = false;
                        GUILayout.Label(IsChinese ? "没有分支信息" : "No Branch Infos");
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(IsChinese ? "版本分支：" : "Version Branch: ", GUILayout.Width(110));
                        mCur_Select_Branch_Index = EditorGUILayout.Popup(mCur_Select_Branch_Index, mAllBranchNames);
                        EditorGUILayout.EndHorizontal();
                        mCurSelectBranchName = mAllBranchNames[mCur_Select_Branch_Index];
                    }
                }
                

            }
            #endregion

            #region 平台选择
            if (mBranchAndPlatformValid)
            {
                if (flag_platform_from_param)
                {
                    //指定了平台，直接显示
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(IsChinese ? "目标平台" : "Platform :",GUILayout.Width(110));
                    GUILayout.Label(platform_from_param.ToString());
                    EditorGUILayout.EndHorizontal();
                    //由分支决定平台
                    if (mCurBranch == null || mCurBranch.BranchName != mCurSelectBranchName)
                    {
                        VFSManagerEditor.VersionManager.TryGetVersionBranch(mCurSelectBranchName, out mCurBranch);
                    }
                }
                else
                {
                    if (flag_branchName_from_param)
                    {
                        //指定了分支但没指定平台，也直接显示
                        //指定了平台，直接显示
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(IsChinese ? "目标平台" : "Platform :", GUILayout.Width(110));
                        GUILayout.Label(platform_from_param.ToString());
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        //由分支决定平台
                        if (mCurBranch == null || mCurBranch.BranchName != mCurSelectBranchName)
                        {
                            VFSManagerEditor.VersionManager.TryGetVersionBranch(mCurSelectBranchName, out mCurBranch);
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(IsChinese ? "目标平台" : "Platform", GUILayout.Width(110));
                        GUILayout.Label(mCurBranch.Platform.ToString());
                        EditorGUILayout.EndHorizontal();
                    }
                    
                }

                if(IsSourcePackagesValidByCurBranch_BranchName == null || IsSourcePackagesValidByCurBranch_BranchName != mCurBranch.BranchName)
                {
                    
                    string platformName = XPlatformUtil.GetNameText(mCurBranch.Platform);
                    string source_packages_folder_path = Path.Combine(VFSEditorConst.PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH, platformName);
                    if (mCurBranch.BType == VersionBranch.BranchType.MainPackage)
                    {
                        IsSourcePackagesValidByCurBranch = VFSEditorUtil.IsValid_MainPackage_InPackages(source_packages_folder_path);
                    }
                    else
                    {
                        IsSourcePackagesValidByCurBranch = VFSEditorUtil.IsValid_ExtensionGroup_InPackages(source_packages_folder_path, mCurBranch.ExtensionGroupName);
                    }

                    IsSourcePackagesValidByCurBranch_BranchName = mCurBranch.BranchName;
                    
                }

                if (!IsSourcePackagesValidByCurBranch)
                {
                    EditorGUILayout.HelpBox(IsChinese ? "没有适合当前分支的已构建文件，请先构建资源。" : "There are no built files for the current branch, please build assets first.", MessageType.Error);
                    mBranchAndPlatformValid = false;
                }
            }
            #endregion

            #region 杂七杂八信息
            if (mBranchAndPlatformValid)
            {
                //先算一下版本号
                if(curMaxVersionCode_BranchName == null|| curMaxVersionCode_BranchName != mCurBranch.BranchName)
                {
                    var max_version = mCurBranch.GetMaxVersion();
                    if(max_version == null)
                    {
                        curMaxVersionCode = -1;
                    }
                    else
                    {
                        curMaxVersionCode = max_version.Value.versionCode;
                    }
                    curMaxVersionCode_BranchName = mCurBranch.BranchName;
                }

                //版本号
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(IsChinese ? "版本号：" : "Version Code:", GUILayout.Width(110));
                mCurVersionCode = EditorGUILayout.LongField(mCurVersionCode);
                EditorGUILayout.EndHorizontal();
                if(mCurVersionCode <= curMaxVersionCode)
                {
                    mCurVersionCode = curMaxVersionCode + 1;
                }

                //版本名
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(IsChinese ? "版本名：" : "Version Name:", GUILayout.Width(110));
                mCurVersionName = EditorGUILayout.TextField(mCurVersionName);
                EditorGUILayout.EndHorizontal();

                //版本描述
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(IsChinese ? "版本描述：" : "Description:", GUILayout.Width(110));
                mCurVersionDecs = EditorGUILayout.TextArea(mCurVersionDecs);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
                //保存二进制文件
                EditorGUILayout.BeginHorizontal();
                //GUILayout.Label(IsChinese ? "保存二进制文件：" : "Save Binary:", GUILayout.Width(125));
                mCurSaveBinary = EditorGUILayout.Toggle(IsChinese ? "保存二进制文件：" : "Save Binary:", mCurSaveBinary);
                EditorGUILayout.EndHorizontal();
                if (mCurSaveBinary)
                {
                    if (IsChinese)
                    {
                        EditorGUILayout.HelpBox("该版本记录不仅仅会保存资源的Hash值，同时将会保存所有构建出的资源文件。这会占用较大的磁盘空间。", MessageType.Info);
                    }
                    else
                        EditorGUILayout.HelpBox("This version record will not only save the hash info of the resource, but also all the resource files that are built. This will take up a lot of disk space.", MessageType.Info);
                }
            }

            if (mBranchAndPlatformValid)
            {
                if (GUILayout.Button("Save"))
                {

                    VFSManagerEditor.VersionManager.AddVersionRecord(mCurBranch.BranchName, mCurVersionCode, mCurVersionName, mCurVersionDecs, mCurSaveBinary);
                    EditorUtility.DisplayDialog("Finish", "Finish", "Ok");
                    this.Close();
                }
            }
            

            #endregion

            EditorGUILayout.EndVertical();
        }

        private void OnLostFocus()
        {
            IsSourcePackagesValidByCurBranch_BranchName = string.Empty;
        }

        private void OnFocus()
        {
            
        }

        private void OnDestroy()
        {
            wnd = null;
        }

    }
}
