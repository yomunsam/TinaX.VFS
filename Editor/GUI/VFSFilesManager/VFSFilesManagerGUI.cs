using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using TinaX;
using TinaX.Utils;
using TinaX.VFSKit;
using TinaX.VFSKitInternal.Utils;
using TinaXEditor.VFSKit.Utils;

namespace TinaXEditor.VFSKit.UI
{
    internal class VFSFilesManagerGUI : EditorWindow
    {
        static VFSFilesManagerGUI wnd;
        public static void OpenUI()
        {
            if (wnd == null)
            {
                wnd = GetWindow<VFSFilesManagerGUI>();
                wnd.titleContent = new GUIContent("VFS Files Manager");
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }

        private int source_packages_counter = 0;
        private int packages_stream_counter = 0; //把stream写在packages后面，是为了防止想输入string时候，ide自动给联想到stream，所以这里的变量命名风格不一致
        private Dictionary<XRuntimePlatform, bool> mDict_MainPackage_SourcePackage;
        private Dictionary<XRuntimePlatform, bool> mDict_MainPackage_StreamingAssets;

        private Dictionary<XRuntimePlatform, bool> mDict_Extensions_SourcePackage; //有任何一个扩展包就为true
        private Dictionary<XRuntimePlatform, bool> mDict_Extensions_StreamingAssets;

        private bool mFlag_RefreshData = false;

        private XRuntimePlatform? mLeft_Select_Platform;
        private XRuntimePlatform? mRight_Select_Platform;

        private int width_left_area = 220;
        private int width_center_area = 250;
        private int width_right_area = 220;
        private int width_total => width_left_area + width_center_area + width_right_area;

        private Vector2 v2_list_left;
        private Vector2 v2_list_right;
        private Vector2 v2_list_center;

        private GUIStyle _style_box;
        private GUIStyle mStyle_box
        {
            get
            {
                if(_style_box == null)
                {
                    _style_box = new GUIStyle(GUI.skin.box);
                    _style_box.margin.left = 5;
                    _style_box.margin.right = 5;
                    _style_box.margin.top = 5;
                    _style_box.margin.bottom = 5;
                }
                return _style_box;
            }
        }

        private GUIStyle _style_box_center;
        private GUIStyle mStyle_box_center
        {
            get
            {
                if (_style_box_center == null)
                {
                    _style_box_center = new GUIStyle(GUI.skin.box);
                    _style_box_center.margin.left = 5;
                    _style_box_center.margin.right = 5;
                    _style_box_center.margin.top = 5;
                    _style_box_center.margin.bottom = 5;
                    _style_box_center.padding.top = 3;
                    _style_box_center.padding.bottom = 3;
                }
                return _style_box;
            }
        }

        private GUIStyle _style_select_label;
        private GUIStyle mStyle_select_label
        {
            get
            {
                if(_style_select_label == null)
                {
                    _style_select_label = new GUIStyle(EditorStyles.label);
                    _style_select_label.normal.textColor = TinaX.Internal.XEditorColorDefine.Color_Normal_Pure;
                    _style_select_label.fontStyle = FontStyle.Bold;
                }
                return _style_select_label;
            }
        }

        private GUIStyle _style_center_large_label;
        private GUIStyle mStyle_center_large_label
        {
            get
            {
                if (_style_center_large_label == null)
                {
                    _style_center_large_label = new GUIStyle(EditorStyles.label);
                    //_style_center_large_label.normal.textColor = TinaX.Internal.XEditorColorDefine.Color_Normal_Pure;
                    _style_center_large_label.fontStyle = FontStyle.Bold;
                    _style_center_large_label.fontSize = 15;
                    _style_center_large_label.alignment = TextAnchor.MiddleCenter;
                }
                return _style_center_large_label;
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

        private void OnDestroy()
        {
            wnd = null;
        }

        private void OnFocus()
        {
            mFlag_RefreshData = false;

        }


        private void OnGUI()
        {
            if (!mFlag_RefreshData) this.RefreshDatas();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(width_total));
            //左边列表-source packages
            EditorGUILayout.BeginVertical(mStyle_box, GUILayout.Width(width_left_area));
            GUILayout.Label("Source Packages:");
            if(source_packages_counter > 0)
            {
                v2_list_left = EditorGUILayout.BeginScrollView(v2_list_left);
                foreach (var item in mDict_MainPackage_SourcePackage)
                {
                    if(item.Value || mDict_Extensions_SourcePackage[item.Key])
                    {
                        if(mLeft_Select_Platform!= null && mLeft_Select_Platform == item.Key)
                        {
                            GUILayout.Label($"[{item.Key.ToString()}]", mStyle_select_label);
                        }
                        else
                        {
                            if (GUILayout.Button(item.Key.ToString()))
                            {
                                mLeft_Select_Platform = item.Key;
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(IsChinese ? "没有任何已构建的资源" : "No any built files", mStyle_center_large_label);
                GUILayout.FlexibleSpace();
            }


            EditorGUILayout.EndVertical();

            //中间
            EditorGUILayout.BeginVertical(GUILayout.Width(width_center_area));

            v2_list_center = EditorGUILayout.BeginScrollView(v2_list_center);
            //中间-source packages
            EditorGUILayout.BeginVertical(mStyle_box_center, GUILayout.MinHeight((this.position.height - 25) / 2));
            if(mLeft_Select_Platform == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(@"(_　_)。゜zｚＺ");
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUILayout.Label(IsChinese ? "已构建的资源" : "Built Assets");
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            //中间-streamingassets
            EditorGUILayout.BeginVertical(mStyle_box_center, GUILayout.MinHeight((this.position.height - 25) / 2));
            if (mRight_Select_Platform == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(@"(๑•̀ㅂ•)و✧́");
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUILayout.Label("StreamingAssets");
                if (GUILayout.Button(IsChinese ? "删除资源" : "Delete Assets"))
                {

                }

            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            //右边
            EditorGUILayout.BeginVertical(mStyle_box, GUILayout.Width(width_right_area));
            GUILayout.Label("StreamingAssets:");
            if(packages_stream_counter > 0)
            {
                v2_list_right = EditorGUILayout.BeginScrollView(v2_list_right);
                foreach (var item in mDict_MainPackage_StreamingAssets)
                {
                    if (item.Value || mDict_Extensions_StreamingAssets[item.Key])
                    {
                        if (mRight_Select_Platform != null && mRight_Select_Platform == item.Key)
                        {
                            GUILayout.Label($"[{item.Key.ToString()}]", mStyle_select_label);
                        }
                        else
                        {
                            if (GUILayout.Button(item.Key.ToString()))
                            {
                                mRight_Select_Platform = item.Key;
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(IsChinese ? "没有任何有效的的包" : "No any valid packages", mStyle_center_large_label);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 准备GUI需要用到的数据
        /// </summary>
        private void RefreshDatas()
        {
            source_packages_counter = 0;
            if (mDict_MainPackage_SourcePackage == null)
                mDict_MainPackage_SourcePackage = new Dictionary<XRuntimePlatform, bool>();
            else
                mDict_MainPackage_SourcePackage.Clear();

            if (mDict_MainPackage_StreamingAssets == null)
                mDict_MainPackage_StreamingAssets = new Dictionary<XRuntimePlatform, bool>();
            else
                mDict_MainPackage_StreamingAssets.Clear();

            if (mDict_Extensions_SourcePackage == null)
                mDict_Extensions_SourcePackage = new Dictionary<XRuntimePlatform, bool>();
            else
                mDict_Extensions_SourcePackage.Clear();

            if (mDict_Extensions_StreamingAssets == null)
                mDict_Extensions_StreamingAssets = new Dictionary<XRuntimePlatform, bool>();
            else
                mDict_Extensions_StreamingAssets.Clear();

            foreach (XRuntimePlatform platform in Enum.GetValues(typeof(XRuntimePlatform)))
            {
                string platform_name = XPlatformUtil.GetNameText(platform);

                //source packages
                string source_packages_path = VFSEditorUtil.GetSourcePackagesFolderPath(platform_name);
                mDict_MainPackage_SourcePackage.Add(platform, VFSEditorUtil.IsValid_MainPackage_InPackages(source_packages_path));
                mDict_Extensions_SourcePackage.Add(platform, VFSEditorUtil.IsAnyValidExtensionGroup_InPackages(source_packages_path));
                if (mDict_MainPackage_SourcePackage[platform] || mDict_Extensions_SourcePackage[platform])
                    source_packages_counter++;

                //streamingassets
                string packages_stream_path = VFSUtil.GetPackagesRootFolderInStreamingAssets(platform_name);
                mDict_MainPackage_StreamingAssets.Add(platform, VFSEditorUtil.IsValid_MainPackage_InPackages(packages_stream_path, true));
                mDict_Extensions_StreamingAssets.Add(platform, VFSEditorUtil.IsAnyValidExtensionGroup_InPackages(packages_stream_path));
                if (mDict_MainPackage_StreamingAssets[platform] || mDict_Extensions_StreamingAssets[platform])
                    packages_stream_counter++;
            }

            mFlag_RefreshData = true;
        }

    }
}
