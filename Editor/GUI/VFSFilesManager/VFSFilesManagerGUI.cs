using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        private bool mFlag_RefreshData_left = false;
        private XRuntimePlatform? mFlag_RefreshData_left_platform = null;
#pragma warning disable IDE0052 // 删除未读的私有成员
#pragma warning disable CS0414 // 删除未读的私有成员
        private bool mFlag_RefreshData_right = false;
#pragma warning restore IDE0052 // 删除未读的私有成员
#pragma warning restore CS0414 // 删除未读的私有成员
        private XRuntimePlatform? mFlag_RefreshData_right_platform = null;

        /*
         * key会列出当前选择平台的source package中所有的扩展组，
         * value为0代表这个组在streamingassets中没有，
         * value 为1代表在streamingasset中有，但build_id不同
         * valud 为2代表在streamingasset中有，并且build_id一致（或者没检查到build_id）
         */
        private Dictionary<string, int> mDict_ExtensionGroups_Info = new Dictionary<string, int>();


        private bool main_package_not_same = false; //source package 和 streamingasset的 main package不一致

        private XRuntimePlatform? mLeft_Select_Platform;
        private XRuntimePlatform? mRight_Select_Platform;

        private int width_left_area = 220;
        private float width_center_area => width_total - width_left_area - width_right_area - 10;
        private int width_right_area = 220;
        private float width_total => this.position.width - 10;

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
                    _style_select_label.alignment = TextAnchor.MiddleCenter;
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
            mFlag_RefreshData_left = false;
            mFlag_RefreshData_right = false;
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
                if (!mFlag_RefreshData_left || mFlag_RefreshData_left_platform == null || mFlag_RefreshData_left_platform.Value != mLeft_Select_Platform.Value) RefreshData_Left_list();
                string platform_name = XPlatformUtil.GetNameText(mLeft_Select_Platform.Value);

                GUILayout.Label(IsChinese ? "已构建的资源" : "Built Assets");
                GUILayout.Label((IsChinese ? "平台：" : "Platform: ") + mLeft_Select_Platform.ToString() + " / " + platform_name);
                if (mDict_MainPackage_SourcePackage[mLeft_Select_Platform.Value])
                {
                    if (!mDict_MainPackage_StreamingAssets[mLeft_Select_Platform.Value])
                    {
                        //source有，stream没有，显示复制选项
                        if (GUILayout.Button(IsChinese?"复制 主包 到StreamingAssets":"Copy Main Package To StreamingAssets"))
                        {
                            VFSEditorUtil.CopyToStreamingAssets(VFSEditorUtil.GetSourcePackagesFolderPath(platform_name), platform_name, false, true);
                            AssetDatabase.Refresh();
                            RefreshDatas();
                        }

                        //source有，stream没有，显示复制选项
                        if (GUILayout.Button(IsChinese ? "复制 全部 到StreamingAssets" : "Copy All Packages To StreamingAssets"))
                        {
                            VFSEditorUtil.CopyToStreamingAssets(VFSEditorUtil.GetSourcePackagesFolderPath(platform_name), platform_name, false, false);
                            AssetDatabase.Refresh();
                            RefreshDatas();
                        }
                    }
                    else
                    {
                        if (main_package_not_same)
                        {
                            EditorGUILayout.HelpBox(IsChinese?"StreamingAssets有 主包 资源，但与构建目录中的资源不一致。": "StreamingAssets has the main package resource, but it is not consistent with the resources in the build directory.", MessageType.None);
                            //source有，stream没有，显示复制选项
                            if (GUILayout.Button(IsChinese ? "复制 主包 到StreamingAssets" : "Copy Main Package To StreamingAssets"))
                            {
                                VFSEditorUtil.CopyToStreamingAssets(VFSEditorUtil.GetSourcePackagesFolderPath(platform_name), platform_name, false, true);
                                AssetDatabase.Refresh();
                                RefreshDatas();
                            }
                        }
                    }
                    
                    
                }
                if (mDict_Extensions_SourcePackage[mLeft_Select_Platform.Value])
                {
                    if (mDict_ExtensionGroups_Info.Count > 0)
                    {
                        GUILayout.Space(5);
                        foreach (var item in mDict_ExtensionGroups_Info)
                        {
                            if (item.Value != 2)
                            {
                                if (GUILayout.Button(IsChinese ? $"复制扩展组 {item.Key} 到StreamingAssets" : $"Copy Extension Group \"{item.Key}\" To StreamingAsssets"))
                                {
                                    string extension_group_source_path = VFSUtil.GetExtensionGroupFolder(VFSEditorUtil.GetSourcePackagesFolderPath(platform_name), item.Key);
                                    VFSEditorUtil.CopyExtensionPackageToSreamingAssets(extension_group_source_path, platform_name, item.Key);
                                    AssetDatabase.Refresh();
                                    RefreshDatas();
                                    RefreshData_Left_list();
                                    Refresh_Right_list();
                                }
                            }
                        }
                    }
                }
                
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
                GUILayout.Label((IsChinese ? "平台：" : "Platform: ") + mRight_Select_Platform.ToString() + " / " + XPlatformUtil.GetNameText(mRight_Select_Platform.Value));
                if (GUILayout.Button(IsChinese ? "删除资源" : "Delete Assets"))
                {
                    if(EditorUtility.DisplayDialog("sure?",IsChinese?"确定要删除吗":"Are you sure to delete?", IsChinese ? "删它！" : "Delete", IsChinese ? "取消" : "Cancel"))
                    {
                        VFSEditorUtil.DeletePackagesFromStreamingAssets(XPlatformUtil.GetNameText(mRight_Select_Platform.Value));
                        mDict_Extensions_StreamingAssets[mRight_Select_Platform.Value] = false;
                        mDict_MainPackage_StreamingAssets[mRight_Select_Platform.Value] = false;
                        mRight_Select_Platform = null;
                        mFlag_RefreshData_right = false;
                        AssetDatabase.Refresh();
                        RefreshDatas();
                        RefreshData_Left_list();
                    }
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

        private void RefreshData_Left_list()
        {
            if (mLeft_Select_Platform == null) return;
            var platform_name = XPlatformUtil.GetNameText(mLeft_Select_Platform.Value);
            string source_packages_root_path = VFSEditorUtil.GetSourcePackagesFolderPath(platform_name);
            string package_stream_root_path = VFSUtil.GetPackagesRootFolderInStreamingAssets(platform_name);
            //检查，StreamingAssets
            if (mDict_MainPackage_SourcePackage[mLeft_Select_Platform.Value] && mDict_MainPackage_StreamingAssets[mLeft_Select_Platform.Value])
            {
                //两边都有，我们来看看两边的build_id是否一致
                string build_info_source_path = VFSUtil.GetMainPackage_BuildInfo_Path(source_packages_root_path);
                string build_info_stream_path = VFSUtil.GetMainPackage_BuildInfo_Path(package_stream_root_path);
                if(File.Exists(build_info_stream_path) && File.Exists(build_info_source_path))
                {
                    try
                    {
                        var build_info_source = JsonUtility.FromJson<TinaX.VFSKitInternal.BuildInfo>(File.ReadAllText(build_info_source_path));
                        var build_info_stream = JsonUtility.FromJson<TinaX.VFSKitInternal.BuildInfo>(File.ReadAllText(build_info_stream_path));
                    
                        if(build_info_source.BuildID != build_info_stream.BuildID)
                        {
                            main_package_not_same = true;
                        }
                    }
                    catch { }
                }
            }
            else
                main_package_not_same = false;

            //扩展组的处理
            mDict_ExtensionGroups_Info.Clear();
            if (mDict_Extensions_SourcePackage[mLeft_Select_Platform.Value])
            {
                string source_extensions_root_path = VFSUtil.GetExtensionPackageRootFolderInPackages(source_packages_root_path);
                string[] group_names = VFSUtil.GetValidExtensionGroupNames(source_extensions_root_path);
                foreach(var group in group_names)
                {
                    //streamingassets 中存在嘛
                    if(VFSUtil.IsValidExtensionPackage(VFSUtil.GetExtensionGroupFolder(package_stream_root_path, group)))
                    {
                        //存在,检查build_id
                        string build_id_path_source = VFSUtil.GetExtensionGroup_BuildInfo_Path(source_packages_root_path,group);
                        string build_id_path_stream = VFSUtil.GetExtensionGroup_BuildInfo_Path(package_stream_root_path, group);
                        try
                        {
                            var b_info_source = JsonUtility.FromJson<TinaX.VFSKitInternal.BuildInfo>(File.ReadAllText(build_id_path_source));
                            var b_info_stream = JsonUtility.FromJson<TinaX.VFSKitInternal.BuildInfo>(File.ReadAllText(build_id_path_stream));
                            if (b_info_source.BuildID == b_info_stream.BuildID)
                                mDict_ExtensionGroups_Info.Add(group, 2);
                            else
                                mDict_ExtensionGroups_Info.Add(group, 1);
                        }
                        catch { }
                        if (!mDict_ExtensionGroups_Info.ContainsKey(group))
                        {
                            mDict_ExtensionGroups_Info.Add(group, 2);
                        }
                    }
                    else
                    {
                        // 不存在
                        mDict_ExtensionGroups_Info.Add(group, 0);
                    }
                }
            }


            mFlag_RefreshData_left_platform = mLeft_Select_Platform;
            mFlag_RefreshData_left = true;
        }

        private void Refresh_Right_list()
        {
            if (mRight_Select_Platform == null) return;


            mFlag_RefreshData_right = true;
            mFlag_RefreshData_right_platform = mRight_Select_Platform;
        }

    }
}
