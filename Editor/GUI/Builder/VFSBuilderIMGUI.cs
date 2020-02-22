using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TinaX;
using TinaX.VFSKit;
using TinaXEditor.VFSKitInternal.I18N;


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



        private string[] xprofiles;
        private int select_xprofile;
        private string cur_select_xprofile_name;
        private XRuntimePlatform cur_select_platform;
        private AssetCompressType cur_select_compress = AssetCompressType.LZ4;
        private bool cur_strictMode = false;
        private bool cur_copyToStreamingAssetFolder = false;

        private void OnDestroy()
        {
            VFSBuilderIMGUI.wnd = null;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(style_body);

            #region Profile 选择
            GUILayout.BeginHorizontal(style_profile_selector);
            GUILayout.Label("Profile:", GUILayout.Width(55));


            if (xprofiles == null || (select_xprofile - 1) > xprofiles.Length)
            {
                refreshXprofilesCacheData();
            }

            select_xprofile = EditorGUILayout.Popup(select_xprofile, xprofiles);
            GUILayout.EndHorizontal();
            #endregion
            cur_select_xprofile_name = xprofiles[select_xprofile];

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
            
            #region 压缩设置
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(VFSBuilderI18N.CopyToStramingAssetPath, GUILayout.MaxWidth(200));
            cur_copyToStreamingAssetFolder = EditorGUILayout.Toggle(cur_copyToStreamingAssetFolder);
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Button("Build", style_btn_build);
            GUILayout.EndVertical();
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
        }

    }
}
