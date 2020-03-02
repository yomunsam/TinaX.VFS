using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using TinaX;


namespace TinaXEditor.VFSKit.Versions
{
    class CreateBranchGUI : EditorWindow
    {
        static CreateBranchGUI wnd;

        [MenuItem("Test/创建分支")]
        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<CreateBranchGUI>();
                wnd.titleContent = new GUIContent("Create Branch");
                wnd.minSize = new Vector2(399, 199);
                wnd.maxSize = new Vector2(400, 200);
                var rect = wnd.position;
                rect.width = 400;
                rect.height = 200;
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


        private XRuntimePlatform mSelect_platform;
        private string mBranchName;
        private VersionBranch.BranchType mSelect_BranchType;
        private string mBranchDesc;

        private string mExtensionGroupName;

        private string[] ExtensionGroupNames = { };
        private int select_extGroup_index;
        private bool flag_refreshExtGroupDatas = false;

        private void OnDestroy()
        {
            wnd = null;
        }

        private void OnEnable()
        {
            if (!VFSManagerEditor.VersionManager.IsBranchExists("master") && mBranchName.IsNullOrEmpty())
            {
                mBranchName = "master";
            }
        }
        private void OnFocus()
        {
            if (flag_refreshExtGroupDatas == false)
                refreshExtensionGroupDatas();
        }

        private void OnLostFocus()
        {
            flag_refreshExtGroupDatas = false;
        }

        private void OnGUI()
        {
            GUILayout.Label(IsChinese ? "创建新分支" : "Create Branch");
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(IsChinese ? "分支名" : "branchName", GUILayout.Width(100));
            mBranchName = EditorGUILayout.TextField(mBranchName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(IsChinese ? "分支类型" : "BranchType", GUILayout.Width(100));
            mSelect_BranchType = (VersionBranch.BranchType)EditorGUILayout.EnumPopup(mSelect_BranchType);
            EditorGUILayout.EndHorizontal();

            if(mSelect_BranchType == VersionBranch.BranchType.ExtensionGroup)
            {
                if (flag_refreshExtGroupDatas == false)
                    refreshExtensionGroupDatas();

                if(ExtensionGroupNames.Length == 0)
                {
                    GUILayout.Label(IsChinese ? "没有可扩展组" : "No Extension Group.");
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(IsChinese ? "扩展组" : "ExtensionGroup:", GUILayout.Width(100));
                    select_extGroup_index = EditorGUILayout.Popup(select_extGroup_index, ExtensionGroupNames);
                    EditorGUILayout.EndHorizontal();
                    mExtensionGroupName = ExtensionGroupNames[select_extGroup_index];
                }

            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(IsChinese ? "目标平台" : "Platform", GUILayout.Width(100));
            mSelect_platform = (XRuntimePlatform)EditorGUILayout.EnumPopup(mSelect_platform);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(IsChinese ? "分支描述" : "description", GUILayout.Width(100));
            mBranchDesc = EditorGUILayout.TextArea(mBranchDesc);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            if (GUILayout.Button(IsChinese ? "新建" : "Create"))
            {
                if (!mBranchName.IsValidFileName())
                {
                    EditorUtility.DisplayDialog(IsChinese ? "有点问题" : "Oops", IsChinese ? "输入的分支名是无效的" : "Branch name you input is invalid.", IsChinese ? "好吧" : "Okey");
                    return;
                }

                //分支名查重
                if (VFSManagerEditor.VersionManager.IsBranchExists(mBranchName))
                {
                    EditorUtility.DisplayDialog(IsChinese ? "有点问题" : "Oops", IsChinese ? "输入的分支名已存在" : "Branch name you input is exists.", IsChinese ? "好吧" : "Okey");
                    return;
                }

                //分支类型和名称确定
                if(mSelect_BranchType == VersionBranch.BranchType.ExtensionGroup && mExtensionGroupName.IsNullOrEmpty())
                {
                    EditorUtility.DisplayDialog(IsChinese ? "有点问题" : "Oops", IsChinese ? "扩展组名称无效" : "Extension group name you input is invalid.", IsChinese ? "好吧" : "Okey");
                    return;
                }

                if (!mBranchDesc.IsNullOrEmpty() && mBranchDesc.Length > 512)
                    mBranchDesc = mBranchDesc.Substring(0, 512);

                if (VFSManagerEditor.VersionManager.AddBranch(mBranchName, mSelect_BranchType, mSelect_platform, mBranchDesc, mExtensionGroupName))
                {
                    EditorUtility.DisplayDialog(IsChinese ? "完成" : "Success", IsChinese ? "创建成功" : "Create branch success.", IsChinese ? "好的" : "Ok");
                    this.Close();
                }
                else
                {
                    this.ShowNotification(new GUIContent(IsChinese ? "创建成功" : "创建失败"));
                }

            }

        }

        private void refreshExtensionGroupDatas()
        {
            ExtensionGroupNames = VFSManagerEditor.GetExtensionGroupNames();
            flag_refreshExtGroupDatas = true;
            select_extGroup_index = 0;
        }
    }
}
