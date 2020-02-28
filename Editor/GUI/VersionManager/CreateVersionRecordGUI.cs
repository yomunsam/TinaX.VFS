using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TinaX;
using UnityEngine;
using UnityEditor;

namespace TinaXEditor.VFSKit.Versions
{
    class CreateVersionRecordGUI : EditorWindow
    {
        #region params
        public static string VFS_Source_Files_Folder { get; set; }
        public static string BranchName { get; set; }
        #endregion

        static CreateVersionRecordGUI wnd;

        [MenuItem("Test/开呀")]
        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<CreateVersionRecordGUI>();
                wnd.titleContent = new GUIContent("VFS Archiver");
                wnd.minSize = new Vector2(399, 499);
                wnd.maxSize = new Vector2(400, 500);
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

        private bool flag_vfs_source_file_folder = false; //标记，是否从上面的静态属性那儿接收了变量
        private string param_vfs_source_file_folder;

        public bool flag_branchName = false;//同上
        public string param_branchName;//同上

        public string[] mBranchNamse;
        public int mSelect_Branch_Index;



        private void OnEnable()
        {
            if (!BranchName.IsNullOrEmpty())
            {
                flag_branchName = true;
                param_branchName = BranchName;
            }
            if (!VFS_Source_Files_Folder.IsNullOrEmpty())
            {
                flag_vfs_source_file_folder = true;
                param_vfs_source_file_folder = VFS_Source_Files_Folder;
            }

            VFS_Source_Files_Folder = string.Empty;
            BranchName = string.Empty;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);
            #region 分支
            if (flag_branchName)
            {
                //指定了分支名
                GUILayout.Label(IsChinese ? "版本分支：" + param_branchName : "Version Branch:" + param_branchName);
            }
            else
            {
                //手动选择分支
                if(mBranchNamse == null || mBranchNamse.Length == 0)
                {
                    mSelect_Branch_Index = 0;
                    mBranchNamse = VFSManagerEditor.VersionManager.GetBranchNames();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(IsChinese ? "版本分支：" : "Version Branch:",GUILayout.MaxWidth(95));
                mSelect_Branch_Index = EditorGUILayout.Popup(mSelect_Branch_Index, mBranchNamse);
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            #region 原始文件位置
            if (flag_vfs_source_file_folder)
            {
                //指定了
                GUILayout.Label("VFS Source Files:" + Path.GetFileName(param_vfs_source_file_folder));
            }
            else
            {
                //手动选择的
                GUILayout.BeginHorizontal();
                GUILayout.Label("VFS Source Files:", GUILayout.Width(120));
                GUILayout.EndHorizontal();
            }
            #endregion


            EditorGUILayout.EndVertical();
        }

        private void OnDestroy()
        {
            wnd = null;
        }

    }
}
