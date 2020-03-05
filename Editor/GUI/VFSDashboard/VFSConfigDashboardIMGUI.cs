using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.EditorTools;
using TinaX;
using TinaX.VFSKit;
using TinaX.VFSKitInternal;
using TinaX.VFSKitInternal.Utils;
using TinaXEditor.Utils;
using System.Linq;
using TinaX.Internal;
using TinaXEditor.VFSKitInternal.I18N;
using TinaXEditor.VFSKit.FileServer;
using TinaXEditor.VFSKit.Versions;

namespace TinaXEditor.VFSKit.UI
{
    public class VFSConfigDashboardIMGUI : EditorWindow
    {
        private static VFSConfigDashboardIMGUI wnd;

        [MenuItem("TinaX/VFS/VFS Dashboard",priority = 10)]
        public static void OpenUI()
        {
            if(wnd == null)
            {
                wnd = GetWindow<VFSConfigDashboardIMGUI>();
                wnd.titleContent = new GUIContent(VFSConfigDashboardI18N.WindowTitle);
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }

        private int Window_Min_Weight = Window_Area_GlobalConfig_Min_Weight + Window_Area_GroupList_Min_Weight + Window_Area_GroupConfig_Min_Weight;
        private const int Window_Area_GlobalConfig_Min_Weight = 300;
        private const int Window_Area_GroupList_Min_Weight = 300;
        private const int Window_Area_GroupConfig_Min_Weight = 400;



        private const string ConfigFileName = "VFSConfig";
        private VFSConfigModel mVFSConfig;
        private SerializedObject mVFSConfigSerializedObject;

        /// <summary>
        /// 相对Resources的路径
        /// </summary>
        private string mConfigFilePath = $"{TinaX.Const.FrameworkConst.Framework_Configs_Folder_Path}/{ConfigFileName}";

        private GUIStyle style_title_h2;
        private GUIStyle style_title_h2_center;
        private GUIStyle style_title_h3;
        private GUIStyle style_text_warning;
        private GUIStyle style_text_normal;

        private GUIStyle _styel_label_color_emphasize;
        private GUIStyle styel_label_color_emphasize
        {
            get
            {
                if(_styel_label_color_emphasize == null)
                {
                    _styel_label_color_emphasize = new GUIStyle(EditorStyles.label);
                    _styel_label_color_emphasize.normal.textColor = XEditorColorDefine.Color_Emphasize;
                }
                return _styel_label_color_emphasize;
            }
        }


        private Vector2 v2_scrollview_globalConfig = Vector2.zero;

        bool b_flodout_global_extname = false;
        private ReorderableList reorderableList_global_extname;

        bool b_flodout_global_pathItem = false;
        private ReorderableList reorderableList_global_pathItem;

        bool b_flodout_global_ab_detail = false;

        bool b_flodout_global_webvfs = false;



        private Vector2 v2_scrollview_assetGroup = Vector2.zero;
        private string input_createGroupName;
        private ReorderableList reorderableList_groups;

        private int? cur_select_group_index;
        private int cur_group_drawing_data_index = -1;
        private ReorderableList reorderableList_groups_folderList;
        private ReorderableList reorderableList_groups_folderSpecialList;
        private ReorderableList reorderableList_groups_assetList;
        private ReorderableList reorderableList_groups_ignoreSubfolder;
        private Vector2 v2_scrollview_assetGroupConfig = Vector2.zero;

        /// <summary>
        /// “文件夹” 图标
        /// </summary>
        private Texture img_folder_icon;
        private Texture img_file_icon;

        private GenericMenu _build_menu;
        private GenericMenu mBuildMenu
        {
            get
            {
                if(_build_menu == null)
                {
                    _build_menu = new GenericMenu();
                    _build_menu.AddItem(
                        new GUIContent(VFSConfigDashboardI18N.Menu_Build_BaseAsset),
                        false,
                        () => { 
                            VFSManagerEditor.RefreshManager(true); 
                            VFSBuilderIMGUI.OpenUI(); 
                        });
                }
                return _build_menu;
            }
        }

        private SerializedProperty sp_enable_vfs;
        /// <summary>
        /// vfs 后缀名
        /// </summary>
        private SerializedProperty sp_vfs_extension;


        private SerializedProperty sp_enable_webvfs;
        private SerializedProperty sp_webvfs_default_download_url;

        private void OnEnable()
        {
            //try to get vfs config
            mVFSConfig = XConfig.GetConfig<VFSConfigModel>(mConfigFilePath);

            this.minSize = new Vector2(Window_Min_Weight, 600);
            VFSManagerEditor.RefreshManager(true);
            


            style_title_h2 = new GUIStyle(EditorStyles.label);
            style_title_h2.fontStyle = FontStyle.Bold;
            style_title_h2.fontSize = 22;
            style_title_h2.normal.textColor = XEditorColorDefine.Color_Normal;
            
            style_title_h2_center = new GUIStyle(EditorStyles.label);
            style_title_h2_center.fontStyle = FontStyle.Bold;
            style_title_h2_center.fontSize = 22;
            style_title_h2_center.alignment = TextAnchor.MiddleCenter;
            style_title_h2_center.normal.textColor = XEditorColorDefine.Color_Normal;


            style_title_h3 = new GUIStyle(EditorStyles.label);
            style_title_h3.fontStyle = FontStyle.Bold;
            style_title_h3.fontSize = 18;
            style_title_h3.normal.textColor = XEditorColorDefine.Color_Normal;

            style_text_warning = new GUIStyle(EditorStyles.label);
            style_text_warning.normal.textColor = XEditorColorDefine.Color_Warning;

            style_text_normal = new GUIStyle(EditorStyles.label);
            style_text_normal.normal.textColor = XEditorColorDefine.Color_Normal;

            img_folder_icon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/io.nekonya.tinax.vfs/Editor/Res/Icons/folder.png");
            img_file_icon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/io.nekonya.tinax.vfs/Editor/Res/Icons/file.png");
        }

        private void OnGUI()
        {
            if (mVFSConfig == null)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Space(35);
                GUILayout.Label("TinaX Virtual File System (VFS) config file not ready. Click \"Create\"button to start use VFS.", style_text_normal);
                if (GUILayout.Button("Create"))
                {
                    mVFSConfig = XConfig.CreateConfigIfNotExists<VFSConfigModel>(mConfigFilePath);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (mVFSConfigSerializedObject == null)
                {
                    mVFSConfigSerializedObject = new SerializedObject(mVFSConfig);
                }


                //绘制顶部工具栏

                #region 旧代码
                //EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                //GUILayout.Label("VFS Editor");

                ////工具栏 -> 配置
                ////EditorGUILayout.BeginHorizontal(EditorStyles.toolbarPopup);
                ////GUILayout.Button("喵");
                ////GUILayout.Button("喵2");
                ////EditorGUILayout.EndHorizontal();
                ////工具栏 -> Build
                //GUILayout.FlexibleSpace();
                //if(GUILayout.Button("Build", EditorStyles.toolbarButton, GUILayout.MinWidth(75), GUILayout.MaxWidth(76)))
                //{
                //    mBuildMenu.ShowAsContext();
                //}

                //EditorGUILayout.EndHorizontal();

                #endregion

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    #region Profile Editor
                    if (GUILayout.Button("Profiles", EditorStyles.toolbarButton, GUILayout.Width(65)))
                    {
                        VFSManagerEditor.RefreshManager(true);
                        ProfileEditorIMGUI.OpenUI();
                    }
                    #endregion
                    GUILayout.FlexibleSpace();

                    #region file server
                    if (!FileServerEditorInstance.IsSupported)
                    {
                        GUILayout.Label(VFSConfigDashboardI18N.Toolbar_FileServer_NotSupport, EditorStyles.toolbarTextField);
                    }
                    else
                    {
                        if (FileServerEditorInstance.IsServerRunning)
                            GUILayout.Label(VFSConfigDashboardI18N.Toolbar_FileServer_Running, EditorStyles.toolbarTextField);
                        else
                            GUILayout.Label(VFSConfigDashboardI18N.Toolbar_FileServer_Stopped, EditorStyles.toolbarTextField);

                        if (GUILayout.Button(VFSConfigDashboardI18N.Toolbar_FileServer_OpenUI, EditorStyles.toolbarButton))
                        {
                            FileServerGUI.OpenUI();
                        }
                    }
                    #endregion
                    EditorGUILayout.Space();
                    #region 版本管理器
                    if (GUILayout.Button(VFSConfigDashboardI18N.Toolbar_VersionMgr, EditorStyles.toolbarButton))
                    {
                        VersionManagerGUI.OpenUI();
                    }
                    #endregion

                    //Build Button
                    if (GUILayout.Button(VFSConfigDashboardI18N.Menu_Build, EditorStyles.toolbarPopup, GUILayout.Width(85)))
                    {
                        mBuildMenu.ShowAsContext();
                    }
                }

                EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(Window_Min_Weight));
                //左边：全局设置
                DrawGlobalConfig();

                //中间： 资源组设置
                DrawAssetsGroupConfig();

                EditorGUILayout.Space(1,true);
                //右侧 资源组细节
                DrawGroupConfig();

                EditorGUILayout.EndHorizontal();

                mVFSConfigSerializedObject.ApplyModifiedProperties(); //只留这边一个

            }
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            VFSManagerEditor.RefreshManager(true);

            wnd = null;
        }

        private void DrawGlobalConfig()
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(450), GUILayout.MinWidth(Window_Area_GlobalConfig_Min_Weight));
            GUILayout.Label("VFS Config", style_title_h2);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            v2_scrollview_globalConfig = EditorGUILayout.BeginScrollView(v2_scrollview_globalConfig);
            //启用vfs
            if(sp_enable_vfs == null)
                sp_enable_vfs = mVFSConfigSerializedObject.FindProperty("EnableVFS");

            //mVFSConfig.EnableVFS = EditorGUILayout.Toggle(VFSConfigDashboardI18N.EnableVFS, mVFSConfig.EnableVFS);
            EditorGUILayout.PropertyField(sp_enable_vfs, new GUIContent(VFSConfigDashboardI18N.EnableVFS));
            EditorGUILayout.Space();
            //忽略后缀名
            b_flodout_global_extname = EditorGUILayout.Foldout(b_flodout_global_extname, VFSConfigDashboardI18N.GlobalVFS_Ignore_ExtName);
            if (b_flodout_global_extname)
            {
                if (reorderableList_global_extname == null)
                {
                    reorderableList_global_extname = new ReorderableList(mVFSConfigSerializedObject,
                                                                         mVFSConfigSerializedObject.FindProperty("GlobalVFS_Ignore_ExtName"),
                                                                         true,
                                                                         true,
                                                                         true,
                                                                         true);
                    reorderableList_global_extname.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_global_extname.serializedProperty.GetArrayElementAtIndex(index);
                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(rect, itemData, GUIContent.none);
                    };
                    reorderableList_global_extname.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, VFSConfigDashboardI18N.GlobalVFS_Ignore_ExtName);
                    };
                    reorderableList_global_extname.onAddCallback = (list) =>
                    {
                        if (list.serializedProperty != null)
                        {
                            list.serializedProperty.arraySize++;
                            list.index = list.serializedProperty.arraySize - 1;

                            SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                            itemData.stringValue = string.Empty;
                        }
                        else
                        {
                            ReorderableList.defaultBehaviours.DoAddButton(list);
                        }
                    };
                    reorderableList_global_extname.onRemoveCallback = (list) =>
                    {
                        SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                        string item = itemData.stringValue.ToLower();
                        if (InternalVFSConfig.GlobalIgnoreExtName.Contains(item))
                        {
                            EditorUtility.DisplayDialog(VFSConfigDashboardI18N.Window_Cannot_delete_internal_config_title, string.Format(VFSConfigDashboardI18N.Window_Cannot_delete_internal_config_content, item), VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                            return;
                        }
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    };
                }

                reorderableList_global_extname.DoLayoutList();
                //mVFSConfigSerializedObject.ApplyModifiedProperties();

            }

            //忽略路径item
            b_flodout_global_pathItem = EditorGUILayout.Foldout(b_flodout_global_pathItem, VFSConfigDashboardI18N.GlobalVFS_Ignore_PathItem);
            if (b_flodout_global_pathItem)
            {
                if (reorderableList_global_pathItem == null)
                {
                    reorderableList_global_pathItem = new ReorderableList(mVFSConfigSerializedObject,
                                                                         mVFSConfigSerializedObject.FindProperty("GlobalVFS_Ignore_Path_Item"),
                                                                         true,
                                                                         true,
                                                                         true,
                                                                         true);
                    reorderableList_global_pathItem.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_global_pathItem.serializedProperty.GetArrayElementAtIndex(index);
                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(rect, itemData, GUIContent.none);
                    };
                    reorderableList_global_pathItem.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, VFSConfigDashboardI18N.GlobalVFS_Ignore_PathItem);
                    };
                    reorderableList_global_pathItem.onAddCallback = (list) =>
                    {
                        if (list.serializedProperty != null)
                        {
                            list.serializedProperty.arraySize++;
                            list.index = list.serializedProperty.arraySize - 1;

                            SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                            itemData.stringValue = string.Empty;
                        }
                        else
                        {
                            ReorderableList.defaultBehaviours.DoAddButton(list);
                        }
                    };
                    reorderableList_global_pathItem.onRemoveCallback = (list) =>
                    {
                        SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                        string item = itemData.stringValue.ToLower();
                        if (InternalVFSConfig.GlobalIgnorePathItemLower.Contains(item))
                        {
                            EditorUtility.DisplayDialog(VFSConfigDashboardI18N.Window_Cannot_delete_internal_config_title, string.Format(VFSConfigDashboardI18N.Window_Cannot_delete_internal_config_content, itemData.stringValue), VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                            return;
                        }
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    };
                }

                reorderableList_global_pathItem.DoLayoutList();
            }

            //AB细节设置
            //AB文件后缀名
            b_flodout_global_ab_detail = EditorGUILayout.Foldout(b_flodout_global_ab_detail, VFSConfigDashboardI18N.Window_AB_Detail);
            if (b_flodout_global_ab_detail)
            {
                if (sp_vfs_extension == null)
                    sp_vfs_extension = mVFSConfigSerializedObject.FindProperty("AssetBundleFileExtension");
                // ab 包后缀名
                GUILayout.BeginHorizontal();
                GUILayout.Label(VFSConfigDashboardI18N.Window_AB_Extension_Name, style_text_normal);
                //EditorGUILayout.PropertyField(sp_vfs_extension, GUILayout.MinWidth(75), GUILayout.MaxWidth(125));
                sp_vfs_extension.stringValue = EditorGUILayout.TextField(sp_vfs_extension.stringValue, GUILayout.MinWidth(75), GUILayout.MaxWidth(125));
                GUILayout.EndHorizontal();
                if (!mVFSConfig.AssetBundleFileExtension.StartsWith("."))
                    GUILayout.Label(VFSConfigDashboardI18N.Window_AB_Extension_Name_Tip_startwithdot, style_text_warning);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();
            }

            //WebVFS
            b_flodout_global_webvfs = EditorGUILayout.Foldout(b_flodout_global_webvfs, "Web VFS");
            if (b_flodout_global_webvfs)
            {
                //enable
                if(sp_enable_webvfs == null)
                    sp_enable_webvfs = mVFSConfigSerializedObject.FindProperty("EnableWebVFS");
                EditorGUILayout.PropertyField(sp_enable_webvfs, new GUIContent(VFSConfigDashboardI18N.Enable_WebVFS));

                //default url
                if(sp_webvfs_default_download_url == null)
                    sp_webvfs_default_download_url = mVFSConfigSerializedObject.FindProperty("DefaultWebVFSBaseUrl");
                EditorGUILayout.LabelField(VFSConfigDashboardI18N.WebVFS_DefaultDownloadUrl);
                EditorGUILayout.PropertyField(sp_webvfs_default_download_url, GUIContent.none);

                EditorGUILayout.HelpBox(VFSConfigDashboardI18N.WebVFS_DefaultDownloadUrl_tips, MessageType.Info);
                if(GUILayout.Button("Configure Urls in Profile"))
                {
                    ProfileEditorIMGUI.param_toolbar_index = 1;
                    ProfileEditorIMGUI.OpenUI();
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();
            }


            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawAssetsGroupConfig()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(Window_Area_GroupList_Min_Weight));
            //小toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            //num
            //GUILayout.Label("Current Groups Num: ");
            //EditorGUILayout.Space();
            input_createGroupName = EditorGUILayout.TextField(input_createGroupName, EditorStyles.toolbarTextField);
            if (GUILayout.Button("Create Group"))
            {
                if (input_createGroupName.IsNullOrEmpty())
                {
                    EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error, VFSConfigDashboardI18N.MsgBox_Msg_CreateGroupNameIsNull, VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                    return;
                }
                //检查重复
                if (mVFSConfig.Groups.Any(g => g.GroupName == input_createGroupName))
                {
                    EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error, string.Format(VFSConfigDashboardI18N.MsgBox_Msg_CreateGroupNameHasExists, input_createGroupName), VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                    return;
                }

                //create
                if (mVFSConfig.Groups == null) mVFSConfig.Groups = new VFSGroupOption[] { };
                List<VFSGroupOption> gs_temp = new List<VFSGroupOption>(mVFSConfig.Groups);
                gs_temp.Add(new VFSGroupOption() { GroupName = input_createGroupName });
                mVFSConfig.Groups = gs_temp.ToArray();
                //mVFSConfigSerializedObject = new SerializedObject(mVFSConfig);

                mVFSConfigSerializedObject.UpdateIfRequiredOrScript();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                VFSManagerEditor.RefreshManager(true);
            }
            EditorGUILayout.EndHorizontal();



            v2_scrollview_assetGroup = EditorGUILayout.BeginScrollView(v2_scrollview_assetGroup);

            if (mVFSConfig.Groups.Length <= 0)
            {
                GUILayout.Label(VFSConfigDashboardI18N.Groups_Item_Null_Tips);
            }
            else
            {
                if (reorderableList_groups == null)
                {
                    reorderableList_groups = new ReorderableList(mVFSConfigSerializedObject,
                                                                         mVFSConfigSerializedObject.FindProperty("Groups"),
                                                                         true,
                                                                         true,
                                                                         false,
                                                                         true);

                    reorderableList_groups.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_groups.serializedProperty.GetArrayElementAtIndex(index);
                        var cur_data = mVFSConfig.Groups[index];
                        rect.y += 2;
                        //name
                        GUI.Label(rect, cur_data.GroupName, EditorStyles.boldLabel);
                        //rect.height = EditorGUIUtility.singleLineHeight;
                        //EditorGUI.PropertyField(rect, itemData, GUIContent.none);

                        if (selected)
                        {
                            cur_select_group_index = index;
                        }
                        else if (cur_select_group_index == index)
                        {
                            cur_select_group_index = null;
                        }
                    };
                    reorderableList_groups.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, "Groups");
                    };
                    reorderableList_groups.onRemoveCallback = (list) =>
                    {
                        if(list.count <= 1)
                        {
                            EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error, VFSConfigDashboardI18N.Groups_Cannot_Be_Null,VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                            return;
                        }
                        //对于删除扩展组的判断
                        var group = reorderableList_groups.serializedProperty.GetArrayElementAtIndex(list.index);
                        if (group.FindPropertyRelative("ExtensionGroup").boolValue)
                        {
                            string group_name = group.FindPropertyRelative("GroupName").stringValue;
                            //获取到该扩展组相关的版本分支
                            string[] branches = VFSManagerEditor.VersionManager.GetBranchNamesByExtensionGroup(group_name);
                            string branch_str = "";
                            if (branches != null && branches.Length > 0)
                            {
                                for (int i = 0; i < branches.Length; i++)
                                {
                                    branch_str += branches[i];
                                    if (i != branches.Length - 1)
                                    {
                                        branch_str += "\n";
                                    }
                                }

                                if (EditorUtility.DisplayDialog("Warning", string.Format(VFSConfigDashboardI18N.Delete_ExtensionGroup_Msg,group_name) + branch_str, "Delete", "Cancel"))
                                {
                                    foreach (var branch in branches)
                                    {
                                        VFSManagerEditor.VersionManager.RemoveBranch(branch);
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }

                        if (cur_select_group_index == list.index)
                            cur_select_group_index = null;
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        VFSManagerEditor.RefreshManager(true);
                    };

                }
                reorderableList_groups.DoLayoutList();

            }


            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawGroupConfig()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(Window_Area_GroupConfig_Min_Weight));

            if(mVFSConfig.Groups == null || mVFSConfig.Groups.Length <= 0)
            {
                cur_select_group_index = null;
            }

            if (cur_select_group_index == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(VFSConfigDashboardI18N.Window_GroupConfig_Null_Tips, style_title_h2_center);
                GUILayout.FlexibleSpace();

                cur_group_drawing_data_index = -1;
            }
            else
            {
                SerializedProperty group_root_property = mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value);

                v2_scrollview_assetGroupConfig = GUILayout.BeginScrollView(v2_scrollview_assetGroupConfig);
                //Group Name
                GUILayout.Label(mVFSConfig.Groups[cur_select_group_index.Value].GroupName, style_title_h3);

                GUILayout.BeginHorizontal();
                //GUILayout.Label(VFSConfigDashboardI18N.Window_GroupConfig_Title_GroupName,GUILayout.MaxWidth(90));
                SerializedProperty groupName = group_root_property.FindPropertyRelative("GroupName");
                if (GUILayout.Button(VFSConfigDashboardI18N.Change_GroupName, GUILayout.Width(110)))
                {
                    EditorGUIUtil.Prompt((success,text) => 
                    {
                        if (success)
                        {
                            if (!text.IsValidFileName()) return;
                            if (text == groupName.stringValue) return;
                            if (mVFSConfig.Groups.Any(g => g.GroupName == text))
                            {
                                UnityEditor.EditorUtility.DisplayDialog("Err", "The Group Name you want modify is already exist: " + text, "Okey");
                                return;
                            }
                            SerializedProperty extension_group = mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value).FindPropertyRelative("ExtensionGroup");
                            if (!extension_group.boolValue)
                            {
                                groupName.stringValue = text;
                            }
                            else
                            {
                                VFSManagerEditor.ChangeExtensionGroupName(groupName.stringValue, text);
                                groupName.stringValue = text;
                                mVFSConfigSerializedObject.ApplyModifiedProperties();
                                VFSManagerEditor.RefreshManager(true);
                            }
                        }
                    }, 
                    title:"Modify Group Name",
                    message:"New Group Name:",
                    defaultContent: groupName.stringValue,
                    comfirn_btn_text:"Modify");
                }
                //EditorGUILayout.PropertyField(groupName, new GUIContent( ));
                GUILayout.EndHorizontal();

                #region Group类型

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                SerializedProperty groupAssetsHandleMode = group_root_property.FindPropertyRelative("GroupAssetsHandleMode");
                EditorGUILayout.PropertyField(groupAssetsHandleMode, new GUIContent(VFSConfigDashboardI18N.Window_Group_HandleMode));
                #endregion

                #region 混淆目录结构
                SerializedProperty obfuscateDirectoryStructure = mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value).FindPropertyRelative("ObfuscateDirectoryStructure");
                EditorGUILayout.PropertyField(obfuscateDirectoryStructure, new GUIContent(VFSConfigDashboardI18N.Window_Group_ObfuscateDirectoryStructure));
                #endregion

                #region 可扩展Groups
                SerializedProperty extensionGroup = group_root_property.FindPropertyRelative("ExtensionGroup");
                //EditorGUILayout.PropertyField(extensionGroup, new GUIContent(VFSConfigDashboardI18N.Window_Group_Extension));
                if (!extensionGroup.boolValue)
                {
                    if (GUILayout.Button(VFSConfigDashboardI18N.Enable_ExtensionGroup, GUILayout.Width(110)))
                    {
                        extensionGroup.boolValue = true;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(VFSConfigDashboardI18N.Is_ExtensionGroup, styel_label_color_emphasize, GUILayout.MaxWidth(240));
                    //GUILayout.Button()
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.LabelField(VFSConfigDashboardI18N.Window_Group_Extensible_Tips,EditorStyles.helpBox);


                #endregion

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


                #region 文件夹列表
                //folder list 
                if (reorderableList_groups_folderList == null || (cur_select_group_index.Value != cur_group_drawing_data_index))
                {
                    reorderableList_groups_folderList = new ReorderableList(mVFSConfigSerializedObject,
                                                                            mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value).FindPropertyRelative("FolderPaths"),
                                                                            true,
                                                                            true,
                                                                            true,
                                                                            true);
                    reorderableList_groups_folderList.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_groups_folderList.serializedProperty.GetArrayElementAtIndex(index);
                        //var cur_data = mVFSConfig.Groups[cur_select_group_index.Value].FolderPaths[index];

                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        var textArea_rect = rect;
                        textArea_rect.width -= 35;

                        EditorGUI.PropertyField(textArea_rect, itemData, GUIContent.none);

                        var btn_rect = rect;
                        btn_rect.y -= 0.5f;
                        btn_rect.x += textArea_rect.width + 2;
                        btn_rect.width = 35;
                        if(GUI.Button(btn_rect, img_folder_icon))
                        {
                            var select_path = EditorUtility.OpenFolderPanel(VFSConfigDashboardI18N.Window_GroupConfig_SelectFolder, "Assets/", "");
                            if (select_path.IsNullOrEmpty()) return;
                            int asset_start_index = select_path.IndexOf("Assets/");
                            if(asset_start_index > -1)
                            {
                                select_path = select_path.Substring(asset_start_index, select_path.Length - asset_start_index);
                            }
                            
                            itemData.stringValue = select_path;
                        }
                    };
                    reorderableList_groups_folderList.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, VFSConfigDashboardI18N.Window_GroupConfig_Title_FolderPaths);
                    };
                    reorderableList_groups_folderList.onAddCallback = (list) =>
                    {
                        // 调整新增item的默认值
                        if (list.serializedProperty != null)
                        {
                            list.serializedProperty.arraySize++;
                            list.index = list.serializedProperty.arraySize - 1;

                            SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                            itemData.stringValue = string.Empty;
                        }
                        else
                        {
                            ReorderableList.defaultBehaviours.DoAddButton(list);
                        }
                    };

                }
                reorderableList_groups_folderList?.DoLayoutList();

                #endregion

                EditorGUILayout.Space();

                #region 特殊文件规则

                if(reorderableList_groups_folderSpecialList == null || (cur_select_group_index.Value != cur_group_drawing_data_index))
                {
                    reorderableList_groups_folderSpecialList = new ReorderableList(mVFSConfigSerializedObject,
                                                                            mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value).FindPropertyRelative("FolderSpecialBuildRules"),
                                                                            true,
                                                                            true,
                                                                            true,
                                                                            true);

                    reorderableList_groups_folderSpecialList.elementHeightCallback = (index) =>
                    {
                        return EditorGUIUtility.singleLineHeight * 4 + 15;
                    };
                    reorderableList_groups_folderSpecialList.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_groups_folderSpecialList.serializedProperty.GetArrayElementAtIndex(index);
                        SerializedProperty folder_path = itemData.FindPropertyRelative("FolderPath");
                        SerializedProperty buildType = itemData.FindPropertyRelative("BuildType");
                        SerializedProperty buildDevType = itemData.FindPropertyRelative("DevType");


                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;

                        Rect label_folder = rect;
                        label_folder.width = 125;
                        EditorGUI.LabelField(label_folder, "Folder Path:");

                        var textArea_rect = rect;
                        textArea_rect.width -= 35;
                        textArea_rect.y += EditorGUIUtility.singleLineHeight;
                        //textArea_rect.x += 5;
                        EditorGUI.PropertyField(textArea_rect, folder_path, GUIContent.none);



                        var btn_rect = rect;
                        btn_rect.y -= 0.5f;
                        btn_rect.y += EditorGUIUtility.singleLineHeight;
                        btn_rect.x += textArea_rect.width + 2;
                        btn_rect.width = 35;
                        if (GUI.Button(btn_rect, img_folder_icon))
                        {
                            var select_path = EditorUtility.OpenFolderPanel(VFSConfigDashboardI18N.Window_GroupConfig_SelectFolder, "Assets/", "");
                            if (select_path.IsNullOrEmpty()) return;
                            int asset_start_index = select_path.IndexOf("Assets/");
                            if (asset_start_index > -1)
                            {
                                select_path = select_path.Substring(asset_start_index, select_path.Length - asset_start_index);
                            }

                            folder_path.stringValue = select_path;
                        }

                        var label_enum_1 = rect;
                        label_enum_1.y += EditorGUIUtility.singleLineHeight * 2 + 4;
                        label_enum_1.width = 80;
                        EditorGUI.LabelField(label_enum_1, "Build Type :");

                        var enum_1_rect = rect;
                        enum_1_rect.y += EditorGUIUtility.singleLineHeight * 2 + 4;
                        enum_1_rect.width -= 85;
                        enum_1_rect.x += 85;
                        EditorGUI.PropertyField(enum_1_rect, buildType,GUIContent.none);

                        var label_enum_2 = rect;
                        label_enum_2.y += EditorGUIUtility.singleLineHeight * 3 + 8;
                        label_enum_2.width = 125;
                        EditorGUI.LabelField(label_enum_2, "Develop Build Type :");

                        var enum_2_rect = rect;
                        enum_2_rect.y += EditorGUIUtility.singleLineHeight * 3 + 8;
                        enum_2_rect.width -= 130;
                        enum_2_rect.x += 130;
                        EditorGUI.PropertyField(enum_2_rect, buildDevType, GUIContent.none);

                    };
                    reorderableList_groups_folderSpecialList.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, VFSConfigDashboardI18N.Window_GroupConfig_Title_SpecialFolder);
                    };
                }

                reorderableList_groups_folderSpecialList?.DoLayoutList();
                #endregion

                EditorGUILayout.Space();

                #region 资源列表
                if (reorderableList_groups_assetList == null || (cur_select_group_index.Value != cur_group_drawing_data_index))
                {
                    reorderableList_groups_assetList = new ReorderableList(mVFSConfigSerializedObject,
                                                                            mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value).FindPropertyRelative("AssetPaths"),
                                                                            true,
                                                                            true,
                                                                            true,
                                                                            true);
                    reorderableList_groups_assetList.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_groups_assetList.serializedProperty.GetArrayElementAtIndex(index);
                        //var cur_data = mVFSConfig.Groups[cur_select_group_index.Value].AssetPaths[index];

                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        var textArea_rect = rect;
                        textArea_rect.width -= 35;

                        EditorGUI.PropertyField(textArea_rect, itemData, GUIContent.none);

                        var btn_rect = rect;
                        btn_rect.y -= 0.5f;
                        btn_rect.x += textArea_rect.width + 2;
                        btn_rect.width = 35;
                        if (GUI.Button(btn_rect, img_file_icon))
                        {


                            var select_path = EditorUtility.OpenFilePanel(VFSConfigDashboardI18N.Window_GroupConfig_SelectAsset, "Assets/", "");
                            if (select_path.IsNullOrEmpty()) return;
                            int asset_start_index = select_path.IndexOf("Assets/");
                            if (asset_start_index > -1)
                            {
                                select_path = select_path.Substring(asset_start_index, select_path.Length - asset_start_index);
                            }

                            if (select_path.ToLower().EndsWith(".meta"))
                            {
                                EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error,
                                                            VFSConfigDashboardI18N.Window_GroupConfig_SelectAsset_Error_Select_Meta,
                                                            VFSConfigDashboardI18N.MsgBox_Common_Confirm) ;
                                return;
                            }

                            itemData.stringValue = select_path;
                        }
                    };
                    reorderableList_groups_assetList.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, VFSConfigDashboardI18N.Window_GroupConfig_Title_AssetPaths);
                    };
                    reorderableList_groups_assetList.onAddCallback = (list) =>
                    {
                        // 调整新增item的默认值
                        if (list.serializedProperty != null)
                        {
                            list.serializedProperty.arraySize++;
                            list.index = list.serializedProperty.arraySize - 1;

                            SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                            itemData.stringValue = string.Empty;
                        }
                        else
                        {
                            ReorderableList.defaultBehaviours.DoAddButton(list);
                        }
                    };

                }
                reorderableList_groups_assetList?.DoLayoutList();
                #endregion

                #region 忽略子目录
                if (reorderableList_groups_ignoreSubfolder == null || (cur_select_group_index.Value != cur_group_drawing_data_index))
                {
                    reorderableList_groups_ignoreSubfolder = new ReorderableList(mVFSConfigSerializedObject,
                                                                         mVFSConfigSerializedObject.FindProperty("Groups").GetArrayElementAtIndex(cur_select_group_index.Value).FindPropertyRelative("IgnoreSubPath"),
                                                                         true,
                                                                         true,
                                                                         true,
                                                                         true);
                    reorderableList_groups_ignoreSubfolder.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_groups_ignoreSubfolder.serializedProperty.GetArrayElementAtIndex(index);

                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        var textArea_rect = rect;
                        textArea_rect.width -= 35;

                        EditorGUI.PropertyField(textArea_rect, itemData, GUIContent.none);

                        var btn_rect = rect;
                        btn_rect.y -= 0.5f;
                        btn_rect.x += textArea_rect.width + 2;
                        btn_rect.width = 35;
                        if (GUI.Button(btn_rect, img_folder_icon))
                        {
                            var select_path = EditorUtility.OpenFolderPanel(VFSConfigDashboardI18N.Window_GroupConfig_SelectFolder, "Assets/", "");
                            if (select_path.IsNullOrEmpty()) return;
                            int asset_start_index = select_path.IndexOf("Assets/");
                            if (asset_start_index > -1)
                            {
                                select_path = select_path.Substring(asset_start_index, select_path.Length - asset_start_index);
                            }

                            //检查所选择的目录是否包含在Folder配置中
                            bool flag = false;
                            foreach(var folder in mVFSConfig.Groups[cur_select_group_index.Value].FolderPaths)
                            {
                                if (VFSUtil.IsSameOrSubPath(select_path, folder, false))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error, string.Format(VFSConfigDashboardI18N.Window_Group_IgnoreSubFolder_MsgBox_NotSubfolder,select_path), VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                                return;
                            }

                            itemData.stringValue = select_path;
                        }
                    };
                    reorderableList_groups_ignoreSubfolder.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, VFSConfigDashboardI18N.Window_Group_IgnoreSubFolder);
                    };
                    reorderableList_groups_ignoreSubfolder.onAddCallback = (list) =>
                    {
                        if (list.serializedProperty != null)
                        {
                            list.serializedProperty.arraySize++;
                            list.index = list.serializedProperty.arraySize - 1;

                            SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(list.index);
                            itemData.stringValue = string.Empty;
                        }
                        else
                        {
                            ReorderableList.defaultBehaviours.DoAddButton(list);
                        }
                    };

                }

                reorderableList_groups_ignoreSubfolder.DoLayoutList();

                #endregion

                #region 忽略后缀名


                #endregion

                cur_group_drawing_data_index = cur_select_group_index.Value;
                //mVFSConfigSerializedObject.ApplyModifiedProperties();
                GUILayout.EndScrollView();
            }
            //Group
            //GUILayout.Label("Group: " + )


            EditorGUILayout.EndVertical();
        }

    }



}

