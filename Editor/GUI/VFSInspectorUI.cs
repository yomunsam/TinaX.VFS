using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TinaXEditor.VFSKit.Utils;
using TinaX.VFSKit.Const;
using TinaXEditor.Utils;
using System;
using System.Linq;

namespace TinaXEditor.VFSKit.UI
{
    using Object = UnityEngine.Object;

    [InitializeOnLoad]
    static class VFSInspectorUI
    {
        static VFSInspectorUI()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        private static GUIStyle _style_bg;
        private static GUIStyle style_bg
        {
            get
            {
                if(_style_bg == null)
                {
                    _style_bg = new GUIStyle(EditorStyles.helpBox);
                }
                return _style_bg;
            }
        }

        private static GUIStyle _style_txt_bold;
        private static GUIStyle style_txt_bold
        {
            get
            {
                if (_style_txt_bold == null)
                {
                    _style_txt_bold = new GUIStyle(EditorStyles.label);
                    _style_txt_bold.fontStyle = FontStyle.Bold;
                    _style_txt_bold.padding.left = 6;
                }
                return _style_txt_bold;
            }
        }
        
        private static GUIStyle _style_btn;
        private static GUIStyle style_btn
        {
            get
            {
                if (_style_btn == null)
                {
                    _style_btn = new GUIStyle();
                    _style_btn.normal.textColor = TinaX.Internal.XEditorColorDefine.Color_Normal;
                    _style_btn.padding.left = 20;
                    _style_btn.padding.right = 20;
                }
                return _style_btn;
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

        private static string mCurShowAssetPath;
        private static string mCurSelectedAssetPath;
        private static AssetsStatusQueryResult mCurAssetData;

        private static void OnPostHeaderGUI(Editor editor)
        {
            if(editor.targets.Length == 1)
            {
                string guid = string.Empty;
                if(VFSEditorUtil.GetPathAndGUIDFromTarget(editor.target,out string path,ref guid, out Type mainAssetType))
                {
                    mCurSelectedAssetPath = path;
                    

                    if (!IsTypeIgnore(mainAssetType) && path.StartsWith("Assets/"))
                    {
                        if (mCurSelectedAssetPath != mCurShowAssetPath)
                        {
                            //获取数据
                            VFSManagerEditor.QueryAsset(path, out mCurAssetData, false);
                            mCurShowAssetPath = mCurSelectedAssetPath;
                        }

                        GUILayout.Space(10);
                        
                        EditorGUILayout.BeginVertical(style_bg);
                        EditorGUILayout.LabelField("VFS Preview", EditorStyles.centeredGreyMiniLabel);
                        if (mCurAssetData.ManagedByVFS)
                        {
                            GUILayout.Label("Managed By VFS.", style_txt_bold);
                            
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Group : ", style_txt_bold, GUILayout.MaxWidth(95));
                            EditorGUILayout.LabelField(mCurAssetData.GroupName);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Build Type :", style_txt_bold, GUILayout.MaxWidth(95));
                            EditorGUILayout.LabelField(mCurAssetData.BuildType.ToString());
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Develop Type :", style_txt_bold, GUILayout.MaxWidth(95));
                            EditorGUILayout.LabelField(mCurAssetData.DevType.ToString());
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("AB Path : ", style_txt_bold, GUILayout.MaxWidth(65));
                            EditorGUILayout.LabelField(mCurAssetData.AssetBundleFileName);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(IsChinese?"复制路径":"Copy Path",GUILayout.Width(80)))
                            {
                                GUIUtility.systemCopyBuffer = path;
                            }
                            GUILayout.EndHorizontal();

                        }
                        else
                        {
                            
                            GUILayout.Label("Non VFS managed asset.");
                            GUILayout.BeginHorizontal(style_btn);
                            if (GUILayout.Button("Dashboard"))
                            {
                                VFSConfigDashboardIMGUI.OpenUI();
                            }
                            GUILayout.EndHorizontal();
                        }

                        EditorGUILayout.Separator();

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }

        private static bool IsTypeIgnore(Type type)
        {
            return VFSConst.IgnoreType.Any(t => t == type);
        }

    }
}

