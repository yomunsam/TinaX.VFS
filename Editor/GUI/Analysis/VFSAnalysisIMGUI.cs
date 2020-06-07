using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using TinaX;
using TinaX.VFSKit;
using TinaX.VFSKitInternal;

namespace TinaXEditor.VFSKitInternal
{
    public class VFSAnalysisIMGUI : EditorWindow
    {
        static VFSAnalysisIMGUI wnd;

        [MenuItem("TinaX/VFS/VFS AnalysisIMGUI",false,41)]
        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<VFSAnalysisIMGUI>();
                wnd.titleContent = new GUIContent("VFS Analysis");
                //wnd.minSize = new Vector2(424, 599);
                //wnd.maxSize = new Vector2(425, 600);
                //Rect pos = wnd.position;
                //pos.width = 425;
                //pos.height = 600;
                //wnd.position = pos;
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }

        [SerializeField] TreeViewState m_TreeViewState_AssetBundle;
        VFSAnalysisAssetBundleTreeView m_TreeView_AssetBundle;
        SearchField m_SearchField_AssetBundle;

        [SerializeField] TreeViewState m_TreeViewState_EditorAsset;
        VFSAnalysisEditorTreeView m_TreeView_EditorAsset;
        SearchField m_SearchField_EditorAsset;


        private bool refresh_data = false;
        private bool valid_data = false; //数据是否有效
        private bool loadFromAB = false;


        private void refreshData()
        {
            refresh_data = true;
            valid_data = false;
            if (!EditorApplication.isPlaying)
                return;
            if (XCore.MainInstance == null)
                return;

            if(!XCore.MainInstance.TryGetService<IVFSInternal>(out var vfs))
            {
                valid_data = false;
                return;
            }
            if (vfs.LoadFromAssetbundle())
            {
                loadFromAB = true;
                //AssetBundle模式
                if (m_TreeViewState_AssetBundle == null)
                    m_TreeViewState_AssetBundle = new TreeViewState();
                if (m_TreeView_AssetBundle == null)
                    m_TreeView_AssetBundle = new VFSAnalysisAssetBundleTreeView(m_TreeViewState_AssetBundle);
                if (m_SearchField_AssetBundle == null)
                {
                    m_SearchField_AssetBundle = new SearchField();
                    m_SearchField_AssetBundle.downOrUpArrowKeyPressed += m_TreeView_AssetBundle.SetFocusAndEnsureSelectedItem;
                }
                m_TreeView_AssetBundle.Reload();
            }
            else
            {
                loadFromAB = false;
                if (m_TreeViewState_EditorAsset == null)
                    m_TreeViewState_EditorAsset = new TreeViewState();
                if (m_TreeView_EditorAsset == null)
                    m_TreeView_EditorAsset = new  VFSAnalysisEditorTreeView(m_TreeViewState_EditorAsset);
                if (m_SearchField_EditorAsset == null)
                {
                    m_SearchField_EditorAsset = new SearchField();
                    m_SearchField_EditorAsset.downOrUpArrowKeyPressed += m_TreeView_EditorAsset.SetFocusAndEnsureSelectedItem;
                }
                m_TreeView_EditorAsset.Reload();
            }
            
            valid_data = true;
        }

        private void OnGUI()
        {
            if (!refresh_data)
                refreshData();
            DrawToolbar();

            if (!EditorApplication.isPlaying)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(I18Ns.OnlyPlaying, Styles.center_label);
                GUILayout.FlexibleSpace();
                valid_data = false;
            }
            else
            {
                if (!valid_data)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(I18Ns.InvalidData, Styles.center_label);
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    if (loadFromAB)
                    {
                        if(m_TreeView_AssetBundle != null)
                        {
                            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
                            m_TreeView_AssetBundle.OnGUI(rect);

                            EditorGUILayout.BeginVertical(Styles.box);
                            //当前选中项
                            var selects = m_TreeView_AssetBundle.GetSelection();
                            if(selects.Count != 1)
                            {
                                GUILayout.Label("");
                            }
                            else
                            {
                                var select = selects.First();
                                
                                if(select != 1)
                                {
                                    VFSBundle _bundle;
                                    VFSAsset _asset = null;
                                    bool isAsset = false;
                                    m_TreeView_AssetBundle.Dict_Bundle_id.TryGetValue(select, out _bundle);
                                    if(_bundle == null)
                                    {
                                        if (m_TreeView_AssetBundle.Dict_Assets_id.TryGetValue(select, out _asset))
                                        {
                                            isAsset = true;
                                        }
                                    }

                                    if (_bundle != null)
                                    {
                                        GUILayout.Label("Select Bundle:  " + _bundle.AssetBundleName);
                                        GUILayout.Label(I18Ns.RefCounter + _bundle.RefCount);
                                        GUILayout.Label(I18Ns.AssetStatue + _bundle.LoadState.ToString());
                                        if(GUILayout.Button("Log All Assets Name In This Bundle To Console",GUILayout.MaxWidth(300)))
                                        {
                                            if(_bundle.AssetBundle == null)
                                            {
                                                Debug.Log($"AssetBundle <color=#{TinaX.Internal.XEditorColorDefine.Color_Emphasize_16}> {_bundle.AssetBundleName} </color> not load or unloaded.");
                                                return;
                                            }
                                            var _asset_names = _bundle.AssetBundle.GetAllAssetNames();
                                            var _scene_paths = _bundle.AssetBundle.GetAllScenePaths();
                                            Debug.Log("Assets in AssetBundle \"" + _bundle.AssetBundle.name + "\"");
                                            int counter = 0;
                                            foreach(var aname in _asset_names)
                                            {
                                                counter++;
                                                Debug.Log($"    [{counter}]" + aname);
                                            }
                                            if(_scene_paths.Length > 0)
                                            {
                                                Debug.Log("Scenes Path in AssetBundle \"" + _bundle.AssetBundle.name + "\"");
                                                foreach (var _path in _scene_paths)
                                                {
                                                    Debug.Log("    " + _path);
                                                }
                                            }
                                            
                                        }
                                    }
                                    else if(isAsset && _asset != null)
                                    {
                                        GUILayout.Label("Select Asset:  " + _asset.AssetPathLower);
                                        GUILayout.Label(I18Ns.RefCounter + _asset.RefCount);
                                        GUILayout.Label(I18Ns.AssetStatue + _asset.LoadState.ToString());
                                        GUILayout.Label("Group: " + _asset.Group.GroupName);

                                    }
                                }
                                
                            }

                            EditorGUILayout.EndVertical();
                        }
                    }
                    else
                    {
                        if(m_TreeView_EditorAsset != null)
                        {
                            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
                            m_TreeView_EditorAsset.OnGUI(rect);

                            EditorGUILayout.BeginVertical(Styles.box);
                            EditorGUILayout.LabelField(I18Ns.NoAB_And_Dep_In_Editor, Styles.label_emphasize);
                            //当前选中项
                            var selects = m_TreeView_EditorAsset.GetSelection();
                            if (selects.Count != 1)
                            {
                                GUILayout.Label("");
                            }
                            else
                            {
                                var select = selects.First();

                                if (select != 1)
                                {
                                    EditorAsset asset;
                                    m_TreeView_EditorAsset.Dict_Assets_id.TryGetValue(select, out asset);
                                    if (asset != null)
                                    {
                                        GUILayout.Label("Select Asset:  " + asset.AssetPathLower + "  [Editor Asset]");
                                        GUILayout.Label(I18Ns.RefCounter + asset.RefCount);
                                        GUILayout.Label(I18Ns.AssetStatue + asset.LoadState.ToString());
                                    }
                                }

                            }

                            EditorGUILayout.EndVertical();
                        }
                    }
                }
            }
            
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            //GUILayout.Space(100);
            if(GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                refreshData();
            }
            if(valid_data && refresh_data)
            {
                if(GUILayout.Button("UnloadUnusedAssets", EditorStyles.toolbarButton))
                {
                    XCore.MainInstance.GetService<IVFS>().UnloadUnusedAssets();
                    refreshData();
                }
                if (loadFromAB)
                {
                    GUILayout.Label("AssetBundle Mode");
                }
                else
                {
                    GUILayout.Label("AssetDatabase Mode");
                }
            }
            
            GUILayout.FlexibleSpace();
            if(Application.isPlaying && m_TreeView_AssetBundle != null && m_SearchField_AssetBundle != null)
            {
                m_TreeView_AssetBundle.searchString = m_SearchField_AssetBundle.OnToolbarGUI(m_TreeView_AssetBundle.searchString);
            }
            if (Application.isPlaying && m_TreeView_EditorAsset != null && m_SearchField_EditorAsset != null)
            {
                m_TreeView_EditorAsset.searchString = m_SearchField_EditorAsset.OnToolbarGUI(m_TreeView_EditorAsset.searchString);
            }
            GUILayout.EndHorizontal();
        }



        private void OnDestroy()
        {
            wnd = null;
            if(m_TreeView_AssetBundle != null)
            {
                if (m_TreeView_AssetBundle.Dict_Assets_id != null)
                    m_TreeView_AssetBundle.Dict_Assets_id.Clear();
                if (m_TreeView_AssetBundle.Dict_Bundle_id != null)
                    m_TreeView_AssetBundle.Dict_Bundle_id.Clear();
            }
            if(m_TreeView_EditorAsset != null)
            {
                if (m_TreeView_EditorAsset.Dict_Assets_id != null)
                    m_TreeView_EditorAsset.Dict_Assets_id.Clear();
            }
        }


        static class Styles
        {
            private static GUIStyle _center_label;
            public static GUIStyle center_label
            {
                get
                {
                    if(_center_label == null)
                    {
                        _center_label = new GUIStyle(EditorStyles.label);
                        _center_label.alignment = TextAnchor.MiddleCenter;
                        _center_label.wordWrap = true;
                    }
                    return _center_label;
                }
            }

            private static GUIStyle _label_emphasize;
            public static GUIStyle label_emphasize
            {
                get
                {
                    if (_label_emphasize == null)
                    {
                        _label_emphasize = new GUIStyle(EditorStyles.label);
                        _label_emphasize.normal.textColor = TinaX.Internal.XEditorColorDefine.Color_Emphasize;
                        _label_emphasize.wordWrap = true;
                    }
                    return _label_emphasize;
                }
            }

            private static GUIStyle _box;
            public static GUIStyle box
            {
                get
                {
                    if(_box == null)
                    {
                        _box = new GUIStyle(GUI.skin.box);
                    }
                    return _box;
                }
            }

        }

        static class I18Ns
        {
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

            private static bool? _nihongo_desuka;
            private static bool NihongoDesuka
            {
                get
                {
                    if (_nihongo_desuka == null)
                        _nihongo_desuka = (Application.systemLanguage == SystemLanguage.Japanese);
                    return _nihongo_desuka.Value;
                }
            }

            public static string OnlyPlaying
            {
                get
                {
                    if (IsChinese)
                        return "VFS Analysis 只在 应用运行时可用。";
                    if (NihongoDesuka)
                        return "VFS分析は、アプリケーションの実行中にのみ使用できます。";
                    return "VFS Analysis is available only when the application is running.";
                }
            }

            public static string InvalidData
            {
                get
                {
                    if (IsChinese)
                        return "暂无有效数据，请在TinaX VFS服务启动后再次刷新";
                    if (NihongoDesuka)
                        return "有効なデータがありません。TinaX VFSサービスの開始後にもう一度更新してください";
                    return "No valid data, please refresh again after TinaX VFS service starts";
                }
            }

            public static string RefCounter
            {
                get
                {
                    if (IsChinese)
                        return "引用数：";
                    return "Ref Count: ";
                }
            }

            public static string AssetStatue
            {
                get
                {
                    if (IsChinese)
                        return "状态：";
                    return "Status: ";
                }
            }

            public static string NoAB_And_Dep_In_Editor
            {
                get
                {
                    if (IsChinese)
                        return "在编辑器模拟加载资源的模式中，无法查看AssetBundle加载信息和资源依赖信息。";
                    return "In the mode where the editor simulates loading assets, you cannot view AssetBundle status information and assets dependency information.";
                }
            }
        }
    }
}
