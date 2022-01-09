using System;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Packages;
using TinaXEditor.Core.Utils.Localization;
using TinaXEditor.VFS.Managers;
using TinaXEditor.VFS.Querier;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace TinaXEditor.VFS.IMGUIs.Inspector
{
    [InitializeOnLoad]
    static class InspectorUI
    {
        static InspectorUI()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }


        private static Styles _styles;

        private static Styles MyStyles
        {
            get
            {
                if (_styles == null)
                    _styles = new Styles();
                return _styles;
            }
        }

        private static Localizer _localier;

        private static Localizer L
        {
            get
            {
                if (_localier == null)
                    _localier = new Localizer();
                return _localier;
            }
        }

        private static UObject m_AssetMainTypeTargetObject; //下面这个Type是根据哪个UObject获取的
        private static Type m_AssetMainType;

        private static UObject m_AssetQueryResultTargetObject; //下面这个QueryResult是根据哪个UObject查出来的
        private static EditorAssetQueryResult m_AssetQueryResult;

        static void OnPostHeaderGUI(Editor editor)
        {
            if(editor.targets.Length == 1)
            {
                var assetPath = AssetDatabase.GetAssetOrScenePath(editor.target);
                if (m_AssetMainTypeTargetObject == null || m_AssetMainTypeTargetObject != editor.target)
                {
                    m_AssetMainType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                }

                if(m_AssetMainType == typeof(UnityEditor.MonoScript)
                    || m_AssetMainType == typeof(UnityEditor.DefaultAsset)
                    || m_AssetMainType == typeof(VFSConfigAsset))
                {
                    return;
                }
                if (!typeof(AssetImporter).IsAssignableFrom(editor.target.GetType()))
                    return;
                if (!assetPath.StartsWith("Assets/"))
                    return;

                EditorGUILayout.BeginVertical("PreBackground");
                EditorGUILayout.LabelField(L.Title, MyStyles.Title);

                if (EditorVFSManager.AssetQuerier == null)
                {
                    EditorGUILayout.LabelField(L.Label_VFSQuerierNotInit, EditorStyles.label);
                    if(GUILayout.Button(L.Btn_InitializeVFSQuerier, GUILayout.MaxWidth(120)))
                    {
                        EditorVFSManager.InitializeAssetQuerier();
                    }
                }
                else
                {
                    if(m_AssetQueryResultTargetObject == null || m_AssetQueryResultTargetObject != editor.target)
                    {
                        m_AssetQueryResult = EditorVFSManager.QueryAsset(assetPath);
                        m_AssetQueryResultTargetObject = editor.target;
                    }

                    //查询信息
                    EditorGUILayout.LabelField(m_AssetQueryResult.Valid ? L.Label_ManagedByVFS : L.Label_NotManagedByVFS, EditorStyles.boldLabel);
                    if (m_AssetQueryResult.Valid)
                    {
                        //包信息
                        if(m_AssetQueryResult.ManagedPackage != null)
                        {
                            if (m_AssetQueryResult.ManagedByMainPack)
                                EditorGUILayout.LabelField(L.Label_ManagedByMainPackage, EditorStyles.label);
                            else
                            {
                                var packageName = (m_AssetQueryResult.ManagedPackage as VFSExpansionPack)?.PackageName;
                                EditorGUILayout.LabelField(string.Format(L.Label_PackageName, packageName), EditorStyles.label);
                            }
                        }

                        //组信息
                        if (m_AssetQueryResult.ManagedGroup != null)
                            EditorGUILayout.LabelField(string.Format(L.Label_GroupName, m_AssetQueryResult.ManagedGroup.GroupName));

                        //AssetBundleName
                        EditorGUILayout.LabelField(string.Format(L.Label_AssetBundleName, m_AssetQueryResult.AssetBundleName));

                        //FileName in AB
                        EditorGUILayout.LabelField(new GUIContent(string.Format(L.Label_FileNameInAssetBundle, m_AssetQueryResult.FileNameInAssetBundle), "File name in AssetBundle"));


                        if (m_AssetQueryResult.IsVariant)
                        {
                            EditorGUILayout.LabelField(string.Format(L.Label_VariantName, m_AssetQueryResult.VariantName));
                        }

                    }
                }

                EditorGUILayout.EndVertical();
            }
        }



        class Styles
        {
            private bool IsHans = EditorLocalizationUtil.IsHans();

            public GUIStyle Title
            {
                get
                {
                    if (_title == null)
                    {
                        _title = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                        if (IsHans)
                            _title.fontSize += 2;
                        else
                            _title.fontSize += 1;
                    }
                    return _title;
                }
            }

            private GUIStyle _title;
        }

        class Localizer
        {
            private bool IsHans = EditorLocalizationUtil.IsHans();

            public string Title
            {
                get
                {
                    if (IsHans)
                        return "VFS预览";
                    return "VFS Preview";
                }
            }

            public string Label_VFSQuerierNotInit
            {
                get
                {
                    if (IsHans)
                        return "VFS资产查询器尚未初始化";
                    return "VFS asset querier has not been initialized.";
                }
            }

            public string Btn_InitializeVFSQuerier
            {
                get
                {
                    if (IsHans)
                        return "初始化查询器";
                    return "Initialize";
                }
            }

            public string Label_ManagedByVFS
            {
                get
                {
                    if (IsHans)
                        return "该资产受VFS管理";
                    return "This asset is managed by VFS";
                }
            }

            public string Label_NotManagedByVFS
            {
                get
                {
                    if (IsHans)
                        return "该资产不受VFS管理";
                    return "This asset is not managed by VFS";
                }
            }

            public string Label_GroupName
            {
                get
                {
                    if (IsHans)
                        return "资产组：{0}";
                    return "Group:{0}";
                }
            }

            public string Label_PackageName
            {
                get
                {
                    if (IsHans)
                        return "资产包：{0}";
                    return "Assets Package:{0}";
                }
            }

            public string Label_ManagedByMainPackage
            {
                get
                {
                    if (IsHans)
                        return "受主包管理";
                    return "Managed by main package";
                }
            }

            public string Label_AssetBundleName
            {
                get
                {
                    if (IsHans)
                        return "AssetBundle名称：{0}";
                    return "AssetBundle name:{0}";
                }
            }

            public string Label_FileNameInAssetBundle
            {
                get
                {
                    if (IsHans)
                        return "AB中的文件名：{0}";
                    return "File name in AB:{0}";
                }
            }

            public string Label_VariantName
            {
                get
                {
                    if (IsHans)
                        return "变体：{0}";
                    return "Variant:{0}";
                }
            }

        }
    }
}
