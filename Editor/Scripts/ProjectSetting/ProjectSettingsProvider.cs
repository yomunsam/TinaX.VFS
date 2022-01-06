using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Consts;
using TinaXEditor.Core;
using TinaXEditor.Core.Consts;
using TinaXEditor.Core.Utils.Localization;
using TinaXEditor.VFS.IMGUIs.PackageConfigEditor;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFS.ProjectSetting
{
    public class ProjectSettingsProvider
    {
        private static StyleDefine _styles;
        private static StyleDefine Styles
        {
            get
            {
                if (_styles == null)
                    _styles = new StyleDefine();
                return _styles;
            }
        }

        private static Localizer _localizer;
        private static Localizer L
        {
            get
            {
                if (_localizer == null)
                    _localizer = new Localizer();
                return _localizer;
            }
        }

        private static bool _PrepareData; //准备数据
        private static VFSConfigAsset _configAsset;
        private static SerializedObject _configAssetSerializedObject;

        private static SerializedProperty _SP_GlobalAssetConfig; //全局资产配置
        private static SerializedProperty _SP_MainPackage; //全局资产配置


        [SettingsProvider]
        public static SettingsProvider GetVFSSettingsProvider()
            => new SettingsProvider($"{XEditorConsts.ProjectSettingsRootName}/VFS", SettingsScope.Project, new string[] { "TinaX", "VFS", "Assets" })
            {
                label = "VFS",
                guiHandler = (searchContent) =>
                {
                    if (!_PrepareData)
                        PrepareData();

                    EditorGUILayout.BeginVertical(Styles.Body);
                    if(_configAsset == null)
                    {
                        GUILayout.Label(L.NoConfig);
                        if (GUILayout.Button(L.CreateConfigAsset, GUILayout.MaxWidth(120)))
                        {
                            EditorConfigAsset.CreateConfigIfNotExists<VFSConfigAsset>(VFSConsts.DefaultConfigAssetName);
                            PrepareData();
                        }
                    }
                    else
                    {
                        //配置 内容
                        GUILayout.Space(10);

                        //------------ 全局配置 ---------------------------------------------------------------------------
                        EditorGUILayout.LabelField(L.GlobalConfiguration, Styles.Title);
                        EditorGUILayout.PropertyField(_SP_GlobalAssetConfig.FindPropertyRelative("DefaultAssetBundleVariant"), L.DefaultAssetBundleVariant);
                        EditorGUILayout.PropertyField(_SP_GlobalAssetConfig.FindPropertyRelative("IgnoreExtensions"), L.IgnoreExtensions);
                        EditorGUILayout.PropertyField(_SP_GlobalAssetConfig.FindPropertyRelative("IgnoreFolderNames"), L.IgnoreFolderNames);

                        GUILayout.Space(20);
                        //------------ 主包 ---------------------------------------------------------------------------
                        EditorGUILayout.LabelField(L.MainPackage, Styles.Title);
                        if(_SP_MainPackage != null)
                        {
                            if (GUILayout.Button(L.EditMainPackage))
                            {
                                PackageConfigEditorUI.EditPackage(_SP_MainPackage);
                            }
                        }


                        if (_configAssetSerializedObject != null)
                            _configAssetSerializedObject.ApplyModifiedProperties();
                    }

                    EditorGUILayout.EndVertical();
                }
            };


        /// <summary>
        /// 准备数据
        /// </summary>
        private static void PrepareData()
        {
            _configAsset = EditorConfigAsset.GetConfig<VFSConfigAsset>(VFSConsts.DefaultConfigAssetName);
            if(_configAsset != null)
            {
                _configAssetSerializedObject = new SerializedObject(_configAsset);
                _SP_GlobalAssetConfig = _configAssetSerializedObject.FindProperty("GlobalAssetConfig");
                _SP_MainPackage = _configAssetSerializedObject.FindProperty("MainPackage");
            }

            _PrepareData = true;
        }


        class StyleDefine
        {
            bool IsHans = EditorLocalizationUtil.IsHans();
            bool IsJp = EditorLocalizationUtil.IsJapanese();

            public StyleDefine()
            {
                Body = new GUIStyle();
                Body.padding.left = 15;
                Body.padding.right = 15;

                Title = new GUIStyle(EditorStyles.largeLabel);
                if (IsHans || IsJp)
                    Title.fontSize += 2;
                else
                    Title.fontSize += 1;
                Title.fontStyle = FontStyle.Bold;
            }

            public GUIStyle Body;

            public GUIStyle Title;
        }

        class Localizer
        {
            bool IsHans = EditorLocalizationUtil.IsHans();
            bool IsJp = EditorLocalizationUtil.IsJapanese();

            public string NoConfig
            {
                get
                {
                    if (IsHans)
                        return "请创建TinaX VFS 配置文件.";
                    if (IsJp)
                        return "TinaX VFS 構成ファイルを作成してください。";
                    return "Please create a TinaX VFS configuration asset.";
                }
            }

            public string CreateConfigAsset
            {
                get
                {
                    if (IsHans)
                        return "创建配置资产";
                    if (IsJp)
                        return "構成アセットを作成する";
                    return "Create Configuration Asset.";
                }
            }

            public string GlobalConfiguration
            {
                get
                {
                    if (IsHans)
                        return "全局配置";
                    return "Global Configuration";
                }
            }


            private GUIContent _DefaultAssetBundleVariant;
            public GUIContent DefaultAssetBundleVariant
            {
                get
                {
                    if(_DefaultAssetBundleVariant == null)
                    {
                        if(IsHans)
                        {
                            _DefaultAssetBundleVariant = new GUIContent("默认AssetBundle变体名", "通常作为AssetBundle文件的后缀名");
                        }
                        else
                        {
                            _DefaultAssetBundleVariant = new GUIContent("Default AssetBundle Variant", "Usually used as an extension for AssetBundle files");
                        }
                    }
                    return _DefaultAssetBundleVariant;
                }
            }

            private GUIContent _IgnoreExtensions;
            public GUIContent IgnoreExtensions
            {
                get
                {
                    if (_IgnoreExtensions == null)
                    {
                        if (IsHans)
                        {
                            _IgnoreExtensions = new GUIContent("忽略扩展名", "忽略扩展名");
                        }
                        else
                        {
                            _IgnoreExtensions = new GUIContent("Ignore Extensions");
                        }
                    }
                    return _IgnoreExtensions;
                }
            }

            private GUIContent _IgnoreFolderNames;
            public GUIContent IgnoreFolderNames
            {
                get
                {
                    if (_IgnoreFolderNames == null)
                    {
                        if (IsHans)
                        {
                            _IgnoreFolderNames = new GUIContent("忽略文件夹名", "忽略文件夹名");
                        }
                        else
                        {
                            _IgnoreFolderNames = new GUIContent("Ignore Folder Name");
                        }
                    }
                    return _IgnoreFolderNames;
                }
            }

            private GUIContent _MainPackage;
            public GUIContent MainPackage
            {
                get
                {
                    if (_MainPackage == null)
                    {
                        if (IsHans)
                        {
                            _MainPackage = new GUIContent("资产主包", "通常基础资产都存放于此包");
                        }
                        else
                        {
                            _MainPackage = new GUIContent("Assets Main Package", "The underlying assets are usually deposited in this package");
                        }
                    }
                    return _MainPackage;
                }
            }

            public string EditMainPackage
            {
                get
                {
                    if (IsHans)
                        return "编辑主包配置";
                    if (IsJp)
                        return "メインパッケージの設定を編集する";
                    return "Edit main package configuration";
                }
            }

        }
    }
}
