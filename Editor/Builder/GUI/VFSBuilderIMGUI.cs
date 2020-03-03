using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TinaX;
using TinaX.VFSKit;
using TinaXEditor.VFSKitInternal.I18N;
using TinaXEditor.VFSKit.Utils;
using TinaXEditor.Const;
using TinaXEditor.VFSKit.Pipeline;
using TinaXEditor.VFSKit.Pipeline.Builtin;

namespace TinaXEditor.VFSKit.UI
{
    internal class VFSBuilderIMGUI : EditorWindow
    {
        private static VFSBuilderIMGUI wnd;

        //[MenuItem("TinaX/VFS/VFS Dashboard")]
        public static void OpenUI()
        {
            if(wnd == null)
            {
                wnd = GetWindow<VFSBuilderIMGUI>();
                wnd.titleContent = new GUIContent(VFSBuilderI18N.WindowTitle);
                wnd.minSize = new Vector2(364, 599);
                wnd.maxSize = new Vector2(365, 600);
                Rect pos = wnd.position;
                pos.width = 365;
                pos.height = 600;
                wnd.position = pos;
            }
            else
            {
                wnd.Focus();
            }
        }

        private GUIStyle _style_body;
        private GUIStyle style_body
        {
            get
            {
                if (_style_body == null)
                {
                    _style_body = new GUIStyle();
                    _style_body.margin.left = 15;
                    _style_body.margin.right = 15;
                    _style_body.margin.top = 25;
                }
                return _style_body;
            }
        }

        private GUIStyle _style_btn_build;
        private GUIStyle style_btn_build
        {
            get
            {
                if(_style_btn_build == null)
                {
                    _style_btn_build = new GUIStyle(GUI.skin.button);
                    _style_btn_build.fontSize = 15;
                    //_style_btn_build.margin.left = 15;
                    //_style_btn_build.margin.right = 15;
                }
                return _style_btn_build;
            }
        }
        private GUIStyle _style_profile_selector;
        private GUIStyle style_profile_selector
        {
            get
            {
                if (_style_profile_selector == null)
                {
                    _style_profile_selector = new GUIStyle();
                    //_style_profile_selector.margin.left = 15;
                    //_style_profile_selector.margin.right = 15;
                    //_style_profile_selector.padding.top = 25;
                }
                return _style_profile_selector;
            }
        }

        private GUIStyle _style_warning;
        private GUIStyle style_warning
        {
            get
            {
                if (_style_warning == null)
                {
                    _style_warning = new GUIStyle(EditorStyles.label);
                    _style_warning.normal.textColor = TinaX.Internal.XEditorColorDefine.Color_Warning;
                }
                return _style_warning;
            }
        }

        private const string SaveKey_StrictMode = "tinax.vfs.builder.gui.strictMode";
        private const string SaveKey_CopyToStreamingAssets = "tinax.vfs.builder.gui.copyToStreamingAssets";
        private const string SaveKey_ClearABAfter = "tinax.vfs.builder.gui.clearABAfter";
        private const string SaveKey_ClearABBefore = "tinax.vfs.builder.gui.clearABBefore";
        private const string SaveKey_ClearOutputFolder = "tinax.vfs.builder.gui.clearOutputFolder";
        private const string SaveKey_ForceRebuild = "tinax.vfs.builder.gui.forceRebuild";

        private string[] xprofiles;
        private int select_xprofile;
        private string cur_select_xprofile_name;
        private bool mDevelopMode;

        private XRuntimePlatform cur_select_platform;
        private AssetCompressType cur_select_compress = AssetCompressType.LZ4;
        private bool cur_strictMode = false;
        private bool cur_copyToStreamingAssetFolder = false;
        private bool cur_clearAllABSign = false;
        private bool cur_clearAllABSignAfterFinish = false;
        private bool cur_ClearOutputFolder = false;
        private bool cur_ForceRebuild = false;

        private bool mPipeline_ready = false;
        private List<Type> mList_pipeline;
        private Vector2 v2_scroll_pipeline;
        //private string cur_preview_profileName;

        private bool isBuilding = false;

        private void OnDestroy()
        {
            VFSBuilderIMGUI.wnd = null;

            #region 保存编辑器选项
            EditorPrefs.SetBool(SaveKey_StrictMode, cur_strictMode);
            EditorPrefs.SetBool(SaveKey_CopyToStreamingAssets, cur_copyToStreamingAssetFolder);
            EditorPrefs.SetBool(SaveKey_ClearABAfter, cur_clearAllABSignAfterFinish);
            EditorPrefs.SetBool(SaveKey_ClearABBefore, cur_clearAllABSign);
            EditorPrefs.SetBool(SaveKey_ClearOutputFolder, cur_ClearOutputFolder);
            EditorPrefs.SetBool(SaveKey_ForceRebuild, cur_ForceRebuild);
            #endregion
        }

        private void Awake()
        {
            cur_strictMode = EditorPrefs.GetBool(SaveKey_StrictMode, false);
            cur_copyToStreamingAssetFolder = EditorPrefs.GetBool(SaveKey_CopyToStreamingAssets, false);
            cur_clearAllABSign = EditorPrefs.GetBool(SaveKey_ClearABBefore, false);
            cur_clearAllABSignAfterFinish = EditorPrefs.GetBool(SaveKey_ClearABAfter, false);
            cur_ClearOutputFolder = EditorPrefs.GetBool(SaveKey_ClearOutputFolder, false);
            cur_ForceRebuild = EditorPrefs.GetBool(SaveKey_ForceRebuild, false);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(style_body);

            #region Profile 选择

            if (xprofiles == null || (select_xprofile - 1) > xprofiles.Length)
            {
                refreshXprofilesCacheData();
            }
            if (mDevelopMode)
            {
                GUILayout.Label("[Develop Mode]", style_warning);
            }
            GUILayout.BeginHorizontal(style_profile_selector);
            GUILayout.Label("Profile:", GUILayout.Width(55));

            //select_xprofile = EditorGUILayout.Popup(select_xprofile, xprofiles);
            GUILayout.Label(xprofiles[select_xprofile]);
            if (GUILayout.Button(VFSBuilderI18N.SwitchProfile, GUILayout.Width(50)))
            {
                SettingsService.OpenProjectSettings(XEditorConst.ProjectSetting_CorePath);
            }
            GUILayout.EndHorizontal();

            #endregion
            if (xprofiles != null && xprofiles.Length > 0)
            {
                cur_select_xprofile_name = xprofiles[select_xprofile];
            }

            #region 平台选择
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.PlatformTarget, GUILayout.MaxWidth(100));
            cur_select_platform = (XRuntimePlatform)EditorGUILayout.EnumPopup(cur_select_platform);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 严格模式
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.strictMode, GUILayout.Width(100));
            cur_strictMode = EditorGUILayout.Toggle(cur_strictMode);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 压缩设置
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.AssetCompressType, GUILayout.MaxWidth(100));
            cur_select_compress = (AssetCompressType)EditorGUILayout.EnumPopup(cur_select_compress);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 复制到StreamingAssets
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.CopyToStramingAssetPath, GUILayout.MaxWidth(200));
            cur_copyToStreamingAssetFolder = EditorGUILayout.Toggle(cur_copyToStreamingAssetFolder);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 在结束前清理AB标记
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.ClearAllABSignBeforeStart);
            cur_clearAllABSign = EditorGUILayout.Toggle(cur_clearAllABSign);
            EditorGUILayout.EndHorizontal();
            GUILayout.Label(VFSBuilderI18N.ClearAllABSignBeforeStart_Tips, EditorStyles.helpBox);
            #endregion

            #region 在结束后清理AB标记
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.ClearAllABSignAfterFinish);
            cur_clearAllABSignAfterFinish = EditorGUILayout.Toggle(cur_clearAllABSignAfterFinish);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 清理输出目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.ClearOutputFolders);
            cur_ClearOutputFolder = EditorGUILayout.Toggle(cur_ClearOutputFolder);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 强制重构建资源
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.ForceRebuild);
            cur_ForceRebuild = EditorGUILayout.Toggle(cur_ForceRebuild);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region 管线
            if (mPipeline_ready == false) { refreshBuildPipline(); }
            if (mList_pipeline != null && mList_pipeline.Count > 0)
            {
                GUILayout.Space(15);
                TinaXEditor.Utils.EditorGUIUtil.HorizontalLine();
                GUILayout.Label("Builder Pipeline:");
                v2_scroll_pipeline = EditorGUILayout.BeginScrollView(v2_scroll_pipeline);
                foreach(var type in mList_pipeline)
                {
                    if (type.Namespace.IsNullOrEmpty())
                    {
                        GUILayout.Label($"- {type.FullName} [{type.Assembly.ManifestModule.Name}]");
                    }
                    else
                    {
                        GUILayout.Label($"- {type.Namespace}.{type.FullName} [{type.Assembly.ManifestModule.Name}]");
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            
            #endregion

            //#region Preview
            //GUILayout.BeginVertical(style_preview);
            //GUILayout.Label("Build Priview", EditorStyles.centeredGreyMiniLabel);
            //GUILayout.Label("Build Group:");
            //GUILayout.FlexibleSpace();
            //GUILayout.EndVertical();

            //#endregion
            EditorGUILayout.Space();

            if(GUILayout.Button("Build", style_btn_build))
            {
                runBuild();
            }
            if(GUILayout.Button("Clear All AssetBundle Signs"))
            {
                VFSEditorUtil.RemoveAllAssetbundleSigns(true);
            }
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        private void OnLostFocus()
        {
            xprofiles = null;
            select_xprofile = 0;
            cur_select_xprofile_name = null;
            mDevelopMode = false;
            mPipeline_ready = false;
        }

        private void OnFocus()
        {
            xprofiles = null;
            select_xprofile = 0;
            cur_select_xprofile_name = null;
            mDevelopMode = false;
            mPipeline_ready = false;

        }

        void refreshXprofilesCacheData()
        {
            xprofiles = XCoreEditor.GetXProfileNames();
            //get cur index 
            int cur_index = 0;
            string cur_name = XCoreEditor.GetCurrentActiveXProfileName();
            for (var i = 0; i < xprofiles.Length; i++)
            {
                if (xprofiles[i] == cur_name)
                {
                    cur_index = i;
                    break;
                }
            }
            select_xprofile = cur_index;

            mDevelopMode = XCoreEditor.IsXProfileDevelopMode(xprofiles[select_xprofile]);
        }

        private void refreshBuildPipline()
        {
            var interface_type = typeof(IBuildHandler);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(interface_type) && t != typeof(BuilderPipelineHead) && t != typeof(BuilderPipelineLast)))
                .ToArray();
            Dictionary<Type, int> dict_priority = new Dictionary<Type, int>();
            //查找优先级
            foreach(var type in types)
            {
                var priority_attr = type.GetCustomAttribute<TinaX.PriorityAttribute>(true);
                if(priority_attr == null)
                {
                    dict_priority.Add(type, 100);
                }
                else
                {
                    dict_priority.Add(type, priority_attr.Priority);
                }
            }

            List<Type> list_types = new List<Type>(types);
            list_types.Sort((x, y) => dict_priority[x].CompareTo(dict_priority[y]));
            mList_pipeline = list_types;
            mPipeline_ready = true;
        }

        private void Update()
        {
            
        }

        void runBuild()
        {
            if (isBuilding) return;
            isBuilding = true;

            try
            {
                VFSManagerEditor.RefreshManager(true);

                var builder = new VFSBuilder()
                    .UseProfile(cur_select_xprofile_name)
                    .SetConfig(VFSManagerEditor.VFSConfig);

                if (mList_pipeline != null && mList_pipeline.Count > 0)
                {
                    var pipeline = new BuilderPipeline();
                    foreach (var type in mList_pipeline)
                    {
                        pipeline.AddLast(Activator.CreateInstance(type) as IBuildHandler);
                    }
                    builder.UsePipeline(pipeline);
                }
                builder.EnableTipsGUI = true;
                builder.CopyToStreamingAssetsFolder = cur_copyToStreamingAssetFolder;
                builder.ClearAssetBundleSignAfterBuild = cur_clearAllABSignAfterFinish;
                builder.ClearAssetBundleSignBeforeBuild = cur_clearAllABSign;
                builder.ForceRebuild = cur_ForceRebuild;
                builder.ClearOutputFolder = cur_ClearOutputFolder;
                builder.Build(cur_select_platform, cur_select_compress);


                this.ShowNotification(new GUIContent("Build Finish"));
                isBuilding = false;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                isBuilding = false;
            }
            

        }

        
    
    }
}
