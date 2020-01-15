using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.EditorTools;
using TinaX;
using TinaX.VFSKit;
using System.Linq;

namespace TinaXEditor.VFSKit
{
    public class VFSConfigDashboardIMGUI : EditorWindow
   {
        [MenuItem("TinaX/VFS/VFS Dashboard")]
        public static void OpenUI()
        {
            VFSConfigDashboardIMGUI wnd = GetWindow<VFSConfigDashboardIMGUI>();
            wnd.titleContent = new GUIContent(VFSConfigDashboardI18N.WindowTitle);
        }

        private const string ConfigFileName = "VFSConfig";
        private VFSConfigModel mVFSConfig;
        private SerializedObject mVFSConfigSerializedObject;

        /// <summary>
        /// 相对Resources的路径
        /// </summary>
        private string mConfigFilePath = $"{TinaX.Const.FrameworkConst.Framework_Configs_Folder_Path}/{ConfigFileName}";

        private GUIStyle style_title_h2;


        bool b_flodout_global_extname = false;
        private ReorderableList reorderableList_global_extname;

        private Vector2 v2_scrollview_assetGroup = Vector2.zero;
        private string input_createGroupName;
        private ReorderableList reorderableList_groups;


        private void OnEnable()
        {
            //try to get vfs config
            mVFSConfig = XConfig.GetConfig<VFSConfigModel>(mConfigFilePath);


            style_title_h2 = new GUIStyle(EditorStyles.label);
            style_title_h2.fontStyle = FontStyle.Bold;
            style_title_h2.fontSize = 22;
        }

        private void OnGUI()
        {
            if(mVFSConfig == null)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Space(35);
                GUILayout.Label("TinaX Virtual File System (VFS) config file not ready. Click \"Create\"button to start use VFS.");
                if (GUILayout.Button("Create"))
                {
                    mVFSConfig = XConfig.CreateConfigIfNotExists<VFSConfigModel>(mConfigFilePath);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            else
            {
                if(mVFSConfigSerializedObject == null)
                {
                    mVFSConfigSerializedObject = new SerializedObject(mVFSConfig);
                }


                //绘制顶部工具栏
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("VFS Editor");

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                //左边：全局设置
                DrawGlobalConfig();

                //中间： 资源组设置
                DrawAssetsGroupConfig();

                EditorGUILayout.EndHorizontal();
            }
        }



        private void DrawGlobalConfig()
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(450));
            GUILayout.Label("Global", style_title_h2);
            EditorGUILayout.Space();

            //忽略后缀名
            b_flodout_global_extname = EditorGUILayout.Foldout(b_flodout_global_extname,VFSConfigDashboardI18N.GlobalVFS_Ignore_ExtName);
            if (b_flodout_global_extname)
            {
                if(reorderableList_global_extname == null)
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
                }

                reorderableList_global_extname.DoLayoutList();
                mVFSConfigSerializedObject.ApplyModifiedProperties();

            }

            ////Groups
            //EditorGUILayout.Space();
            //GUILayout.Label("Groups", EditorStyles.boldLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawAssetsGroupConfig()
        {
            EditorGUILayout.BeginVertical();
            //小toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            //num
            GUILayout.Label("Current Groups Num: ");
            EditorGUILayout.Space();
            input_createGroupName = EditorGUILayout.TextField(input_createGroupName, EditorStyles.toolbarTextField);
            if(GUILayout.Button("Create Group"))
            {
                if (input_createGroupName.IsNullOrEmpty())
                {
                    EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error, VFSConfigDashboardI18N.MsgBox_Msg_CreateGroupNameIsNull, VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                    return;
                }
                //检查重复
                if(mVFSConfig.Groups.Any(g => g.GroupName == input_createGroupName))
                {
                    EditorUtility.DisplayDialog(VFSConfigDashboardI18N.MsgBox_Common_Error, string.Format(VFSConfigDashboardI18N.MsgBox_Msg_CreateGroupNameHasExists, input_createGroupName), VFSConfigDashboardI18N.MsgBox_Common_Confirm);
                    return;
                }

                //create
                if (mVFSConfig.Groups == null) mVFSConfig.Groups = new VFSGroupOption[] { };
                List<VFSGroupOption> gs_temp = new List<VFSGroupOption>(mVFSConfig.Groups);
                gs_temp.Add(new VFSGroupOption() { GroupName = input_createGroupName });
                mVFSConfig.Groups = gs_temp.ToArray();

                mVFSConfigSerializedObject.UpdateIfRequiredOrScript();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }
            EditorGUILayout.EndHorizontal();


            
            v2_scrollview_assetGroup = EditorGUILayout.BeginScrollView(v2_scrollview_assetGroup);

            if(mVFSConfig.Groups.Length <= 0)
            {
                GUILayout.Label(VFSConfigDashboardI18N.Groups_Item_Null_Tips);
            }
            else
            {
                if(reorderableList_groups == null)
                {
                    reorderableList_groups = new ReorderableList(mVFSConfigSerializedObject,
                                                                         mVFSConfigSerializedObject.FindProperty("Groups"),
                                                                         true,
                                                                         true,
                                                                         true,
                                                                         true);

                    reorderableList_groups.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_groups.serializedProperty.GetArrayElementAtIndex(index);
                        var cur_data = mVFSConfig.Groups[index];
                        rect.y += 2;
                        //name
                        GUI.Label(rect,cur_data.GroupName, EditorStyles.boldLabel);
                        //rect.height = EditorGUIUtility.singleLineHeight;
                        //EditorGUI.PropertyField(rect, itemData, GUIContent.none);
                    };
                    reorderableList_groups.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, "Groups");
                    };
                }
                reorderableList_groups.DoLayoutList();

            }


            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }


   }

    

}

