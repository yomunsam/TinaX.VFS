using System.IO;
using TinaX;
using TinaX.VFSKit.Const;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFSKit.Versions
{
    public class MakePatchGUI : EditorWindow
    {
        static MakePatchGUI wnd;

        private MakePatchGUIParamCache _param;
        private MakePatchGUIParamCache wnd_param
        {
            get
            {
                if(_param == null)
                {
                    _param = ScriptableSingleton<MakePatchGUIParamCache>.instance;
                }
                return _param;
            }
        }

        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<MakePatchGUI>();
                wnd.titleContent = new GUIContent("Make Patch");
                wnd.minSize = new Vector2(349, 399);
                wnd.maxSize = new Vector2(350, 400);
                var rect = wnd.position;
                rect.width = 350;
                rect.height = 400;
                wnd.position = rect;
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
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


        private void OnDestroy()
        {
            wnd = null;
        }

        private GUIStyle _style_title;
        private GUIStyle style_title
        {
            get
            {
                if(_style_title == null)
                {
                    _style_title = new GUIStyle(EditorStyles.label);
                    _style_title.wordWrap = true;
                    _style_title.fontSize = 15;
                    _style_title.alignment = TextAnchor.MiddleCenter;
                    _style_title.fontStyle = FontStyle.Bold;
                }
                return _style_title;
            }
        }

        private GUIStyle _style_label_center;
        private GUIStyle style_label_center
        {
            get
            {
                if(_style_label_center == null)
                {
                    _style_label_center = new GUIStyle(EditorStyles.label);
                    _style_label_center.alignment = TextAnchor.MiddleCenter;
                }
                return _style_label_center;
            }
        }

        private GUIStyle _style_label_selected;
        private GUIStyle style_label_selected
        {
            get
            {
                if (_style_label_selected == null)
                {
                    _style_label_selected = new GUIStyle(EditorStyles.label);
                    _style_label_selected.alignment = TextAnchor.MiddleCenter;
                    _style_label_selected.fontStyle = FontStyle.Bold;
                }
                return _style_label_selected;
            }
        }

        private GUIStyle _style_label_error;
        private GUIStyle style_label_error
        {
            get
            {
                if (_style_label_error == null)
                {
                    _style_label_error = new GUIStyle(EditorStyles.label);
                    _style_label_error.wordWrap = true;
                    _style_label_error.normal.textColor = TinaX.Internal.XEditorColorDefine.Color_Error;
                }
                return _style_label_error;
            }
        }

        private Vector2 v2_scroll;
        private long? select_version;

        private void OnGUI()
        {
            if(wnd_param.branchName == null || wnd_param.version_display_list== null || wnd_param.version_code_list == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(IsChinese ? "启动参数丢失，请重新打开此窗口" : "Startup parameters are missing, please reopen this window", style_title);
                GUILayout.FlexibleSpace();
                return;
            }

            EditorGUILayout.BeginVertical();
            if(wnd_param.version_display_list.Length <= 1)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(IsChinese ? "当前分支下并没有可供制作补丁的版本记录" : "There is no version record available for make patch in the current branch", style_title);
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUILayout.Label((IsChinese ? "分支：" : "Branch:") + wnd_param.branchName);
                GUILayout.Label((IsChinese ? "当前版本：" : "Current Version:") + wnd_param.current_version);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(IsChinese?"选择目标版本":"Select Target Version", EditorStyles.centeredGreyMiniLabel);
                v2_scroll = EditorGUILayout.BeginScrollView(v2_scroll);
                for(int i = 0; i < wnd_param.version_display_list.Length; i++)
                {
                    
                    if(wnd_param.version_code_list[i] == wnd_param.current_version)
                    {
                        GUILayout.Label($"**{wnd_param.version_display_list[i]}", style_label_center);
                    }
                    else
                    {
                        if (select_version != null && select_version.Value == wnd_param.version_code_list[i])
                        {
                            GUILayout.Label(wnd_param.version_display_list[i], style_label_selected);
                        }
                        else
                        {
                            if (GUILayout.Button(wnd_param.version_display_list[i]))
                            {
                                select_version = wnd_param.version_code_list[i];
                            }
                        }
                        
                    }
                }

                EditorGUILayout.EndScrollView();

                
                EditorGUILayout.EndVertical();

                if (select_version != null)
                {
                    if (select_version.Value >= wnd_param.current_version)
                    {
                        GUILayout.Label(IsChinese ? "所选目标版本不可大于当前版本" : "The selected target version cannot be greater than the current version", style_label_error);
                    }
                    else
                    {
                        if (GUILayout.Button(IsChinese ? "制作补丁" : "Make Patch"))
                        {
                            DoMake();
                        }
                    }
                }
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndVertical();

        }


        private void DoMake()
        {
            string patch_ext = VFSConst.Patch_File_Extension;
            if (patch_ext.StartsWith("."))
                patch_ext = patch_ext.Substring(1, patch_ext.Length - 1);
            string current_project_dir = Directory.GetCurrentDirectory();
            string save_dir = EditorPrefs.GetString("TINAX_VFS_PATCH_OUTPUT_DIR_CACHE", current_project_dir);
            
            string save_path = EditorUtility.SaveFilePanel(
                IsChinese ? "保存补丁位置" : "Save Patch Path",
                save_dir,
                $"{wnd_param.current_version}",
                patch_ext);

            if (save_path.IsNullOrEmpty()) return;
            if (!Directory.Exists(Path.GetDirectoryName(save_path)))
            {
                Debug.LogError("Folder Not Found:" + Path.GetDirectoryName(save_path));
                return;
            }
            EditorPrefs.SetString("TINAX_VFS_PATCH_OUTPUT_DIR_CACHE", Path.GetDirectoryName(save_path));

            var patchgen = new PatchGenerator();
            patchgen.MakePatchFromVersionLibrary(wnd_param.branchName, wnd_param.current_version, select_version.Value,save_path);
        }

    }

    public class MakePatchGUIParamCache : ScriptableSingleton<MakePatchGUIParamCache>
    {
        public string branchName;
        public string[] version_display_list;
        public long[] version_code_list;
        public long current_version;
    }

}
