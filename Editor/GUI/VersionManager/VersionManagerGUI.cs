using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using TinaX;
using TinaXEditor.Utils;
using TinaXEditor.VFSKit;

namespace TinaXEditor.VFSKit.Versions
{
    class VersionManagerGUI : EditorWindow
    {
        static VersionManagerGUI wnd;
        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<VersionManagerGUI>();
                wnd.titleContent = new GUIContent("VFS Version Manager");
                wnd.minSize = new Vector2(600, 400);
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



        private float branchListAreaWidth
        {
            get
            {
                if(this.position.width < 900)
                {
                    return this.position.width / 3;
                }
                else
                {
                    return 300;
                }
            }
        }
        private float versionListAreaWidth
        {
            get
            {
                if(this.position.width < 900)
                {
                    return this.position.width / 3;
                }
                else
                {
                    return 300;
                }
            }
        }

        private GUIStyle _style_branchList;
        private GUIStyle style_branchList
        {
            get
            {
                if(_style_branchList == null)
                {
                    _style_branchList = new GUIStyle(EditorStyles.helpBox);
                    //_style_branchList = new GUIStyle(GUI.skin.box);
                    _style_branchList.margin.left = 5;
                    _style_branchList.margin.right = 5;
                    _style_branchList.margin.top = 5;
                    _style_branchList.margin.bottom = 15;
                }
                return _style_branchList;
            }
        }
        private GUIStyle _style_versionList;
        private GUIStyle style_versionList
        {
            get
            {
                if(_style_versionList == null)
                {
                    _style_versionList = new GUIStyle(GUI.skin.box);
                    _style_versionList.margin.left = 5;
                    _style_versionList.margin.right = 5;
                    _style_versionList.margin.top = 5;
                    _style_versionList.margin.bottom = 5;
                }
                return _style_versionList;
            }
        }

        private GUIStyle _style_branch_list_selected;
        private GUIStyle style_branch_list_selected
        {
            get
            {
                if (_style_branch_list_selected == null)
                {
                    _style_branch_list_selected = new GUIStyle(EditorStyles.label);
                    _style_branch_list_selected.fontStyle = FontStyle.Bold;
                    _style_branch_list_selected.alignment = TextAnchor.MiddleCenter;
                }
                return _style_branch_list_selected;
            }
        }

        private GUIStyle _style_common_list_center_tips_label;
        private GUIStyle style_common_list_center_tips_label
        {
            get
            {
                if(_style_common_list_center_tips_label == null)
                {
                    _style_common_list_center_tips_label = new GUIStyle(EditorStyles.label);
                    _style_common_list_center_tips_label.alignment = TextAnchor.MiddleCenter;
                    _style_common_list_center_tips_label.fontStyle = FontStyle.Bold;
                }
                return _style_common_list_center_tips_label;
            }
        }

        private string[] mBranchNames;
        private string mSelectBranchName;
        private Vector2 v2_scroll_branch_list;

        private Vector2 v2_scroll_version_list;

        private VersionBranch mSelectedBranchObj;


        private void OnEnable()
        {
            mBranchNames = VFSManagerEditor.VersionManager.GetBranchNames();
            mSelectBranchName = string.Empty;
            mSelectedBranchObj = null;
        }

        private void OnFocus()
        {
            if(mBranchNames == null || mBranchNames.Length == 0)
            {
                mBranchNames = VFSManagerEditor.VersionManager.GetBranchNames();
                if(mBranchNames == null || mBranchNames.Length == 0)
                {
                    mSelectBranchName = string.Empty;
                    mSelectedBranchObj = null;
                }
            }
            if(VFSManagerEditor.VersionManager.GetBranchNames().Length != mBranchNames.Length)
            {
                mBranchNames = VFSManagerEditor.VersionManager.GetBranchNames();
                if (!mBranchNames.Contains(mSelectBranchName))
                {
                    mSelectBranchName = string.Empty;
                    mSelectedBranchObj = null;
                }
            }
        }

        private void OnLostFocus()
        {
            
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            //左边栏，分支列表
            #region 分支列表
            
            EditorGUILayout.BeginVertical(GUILayout.Width(branchListAreaWidth));
            //标题
            EditorGUILayout.LabelField(IsChinese ? "版本分支" : "Version branches");
            EditorGUILayout.BeginVertical(style_branchList);
            
            if (mBranchNames == null || mBranchNames.Length == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(IsChinese ? "没有分支信息" : "No branch info.", style_common_list_center_tips_label);
                if(GUILayout.Button(IsChinese?"创建新分支":"Create One"))
                {
                    CreateBranchGUI.OpenUI();
                }
                GUILayout.FlexibleSpace();
            }
            else
            {
                v2_scroll_branch_list = EditorGUILayout.BeginScrollView(v2_scroll_branch_list);

                foreach (var item in mBranchNames)
                {
                    string branchName = item;
                    if (!mSelectBranchName.IsNullOrEmpty() && mSelectBranchName == item)
                    {
                        GUILayout.Label(item, style_branch_list_selected);
                    }
                    else
                    {
                        if (GUILayout.Button(item))
                        {
                            //获取数据
                            if(VFSManagerEditor.VersionManager.TryGetVersionBranch(branchName, out mSelectedBranchObj))
                            {
                                mSelectBranchName = branchName;
                            }
                            else
                            {
                                this.ShowNotification(new GUIContent(IsChinese?$"获取分支“{branchName}”的信息失败":$"Faild to get infos by branch name: {branchName}"));
                            }
                        }
                    }
                }

                EditorGUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create",GUILayout.Width(65)))
                {
                    CreateBranchGUI.OpenUI();
                }
                if(mSelectBranchName != null && mSelectedBranchObj!= null)
                {
                    if(GUILayout.Button("Delete", GUILayout.Width(65)))
                    {
                        if (EditorUtility.DisplayDialog(IsChinese ? "确定吗？" : "Are you sure", IsChinese ? $"您将要删除分支\"{mSelectBranchName}\"\n继续删除操作将意味着你将失去该分支下的所有版本记录和二进制数据\n并且该操作是不可撤销的，真的要继续吗？" : $"You are about to delete branch \"{mSelectBranchName}\"\nContinuing the delete operation will mean that you will lose all version records and binary data under this branch and the operation is irrevocable. \nDo you really want to continue?", IsChinese ? "继续删除" : "Delete It", IsChinese ? "取消" : "Cancel"))
                        {
                            if (EditorUtility.DisplayDialog(IsChinese ? "二次确认" : "Ask Again", IsChinese ? $"您将要删除分支\"{mSelectBranchName}\"\n继续删除操作将意味着你将失去该分支下的所有版本记录和二进制数据\n并且该操作是不可撤销的，真的要继续吗？\n这是执行操作前的最后一次询问，该操作不可逆。" : $"You are about to delete branch \"{mSelectBranchName}\"\nContinuing the delete operation will mean that you will lose all version records and binary data under this branch and the operation is irrevocable. \nDo you really want to continue?\nThis is the last inquiry before performing the operation", IsChinese ? "继续删除" : "Delete It", IsChinese ? "取消" : "Cancel"))
                            {
                                VFSManagerEditor.VersionManager.RemoveBranch(mSelectBranchName);
                                mBranchNames = VFSManagerEditor.VersionManager.GetBranchNames();
                                mSelectedBranchObj = null;
                                mSelectBranchName = null;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            #endregion

            #region 版本列表
            EditorGUILayout.BeginVertical(GUILayout.Width(versionListAreaWidth));
            //标题
            EditorGUILayout.LabelField(IsChinese ? "版本列表" : "Versions list");
            EditorGUILayout.BeginVertical(style_versionList);
            if(mSelectedBranchObj == null || mSelectBranchName == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(IsChinese ? "请选择分支" : "Please select a branch", style_common_list_center_tips_label);
                GUILayout.FlexibleSpace();
            }
            else
            {
                if (mSelectedBranchObj.VersionRecords_ReadWrite.Count == 0)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(IsChinese ? "没有版本信息记录" : "No version records", style_common_list_center_tips_label);
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    v2_scroll_version_list = EditorGUILayout.BeginScrollView(v2_scroll_version_list);


                    EditorGUILayout.EndScrollView();
                }
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            #endregion

            #region 分支详情和版本操作

            EditorGUILayout.BeginVertical();

            GUILayout.Label(IsChinese ? "分支信息" : "branch info");
            if(mSelectBranchName != null && mSelectedBranchObj != null)
            {
                //分支名
                GUILayout.Label((IsChinese ? "分支名：" : "Branch Name : ") + mSelectedBranchObj.BranchName);
                //平台
                GUILayout.Label((IsChinese ? "目标平台：" : "Platform : ") + mSelectedBranchObj.Platform.ToString());
                //分支类型
                GUILayout.Label((IsChinese ? "分支类型：" : "Branch Type : ") + mSelectedBranchObj.BType.ToString());
                if(mSelectedBranchObj.BType == VersionBranch.BranchType.ExtensionGroup)
                {
                    GUILayout.Label((IsChinese ? "扩展组名：" : "Extension Group Name : ") + mSelectedBranchObj.ExtensionGroupName.ToString());
                }
                //描述
                GUILayout.Label((IsChinese ? "分支描述：" : "Branch Desc : "));
                GUILayout.Label(mSelectedBranchObj.Desc);
                EditorGUIUtil.HorizontalLine();


                //------------具体版本操作------
                if(mSelectedBranchObj.VersionRecords_ReadWrite.Count > 0)
                {
                    GUILayout.Label("Todo 刷新版本");
                }
                else
                {

                }

                //------------版本列表操作----
                if (GUILayout.Button("创建新版本"))
                {
                    CreateVersionRecordGUI.BranchName = mSelectBranchName;
                    CreateVersionRecordGUI.OpenUI();
                }

            }
            else
            {
                GUILayout.Label(" - ");
            }
            

            EditorGUILayout.EndVertical();
            #endregion


            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            wnd = null;
        }
    }
}
