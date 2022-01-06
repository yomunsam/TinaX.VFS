using System.Collections.Generic;
using TinaX;
using TinaX.Core.Helper.String;
using TinaXEditor.Core.Utils.Localization;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.IMGUIs.PackageConfigEditor
{

    /// <summary>
    /// VFS 配置资产 包 编辑器UI
    /// </summary>
    public class PackageConfigEditorUI : EditorWindow
    {
        //------------静态字段------------------------------------------------------
        public static SerializedProperty? PackageToEdit; //打开窗口前，把包对象赋值到这儿
        private static Dictionary<SerializedProperty, PackageConfigEditorUI> _editorWindows = new Dictionary<SerializedProperty, PackageConfigEditorUI>();

        //------------静态公开方法------------------------------------------------------------------------

        public static void EditPackage(SerializedProperty packageSerializedProperty)
        {
            if(_editorWindows.TryGetValue(packageSerializedProperty, out var editorWindows))
            {
                if(editorWindows.m_PackageToEdit == null)
                {
                    _editorWindows.Remove(packageSerializedProperty);
                }
                else
                {
                    editorWindows.Focus();
                    return;
                }
            }
            PackageToEdit = packageSerializedProperty;
            var window = GetWindow<PackageConfigEditorUI>();
            _editorWindows.Add(packageSerializedProperty, window);
        }

        //------------私有字段--------------------------------------------------------
        private StyleDefine? Styles;
        private Localizer? L;
        public SerializedProperty? m_PackageToEdit; //当前窗口对象要编辑的内容

        private bool m_Initialized = false;
        private SerializedProperty? m_SP_Groups;

        private SerializedProperty? m_SelectedGroup; //当前被选中的组
        private int? m_SelectedGroupByIndex; //上面这个对象是根据哪个index获得的

        private GUIContent[]? m_GroupNames;

        private int m_SelectedGroupIndex;
        //------------生命周期方法------------------------------------------------------

        private void OnEnable()
        {
            if (m_PackageToEdit == null && PackageToEdit != null)
            {
                m_PackageToEdit = PackageToEdit;
            }
            Initialize();
        }

        private void OnGUI()
        {
            if (!m_Initialized)
                Initialize();

            if (Styles == null)
                Styles = new StyleDefine();
            if (L == null)
                L = new Localizer();

            if (m_PackageToEdit == null)
            {
                GUILayout.Label(L!.EditrObjectMissing, Styles!.Title_Error);
                return;
            }
            else
            {

                //EditorGUILayout.BeginVertical("PreBackground");
                EditorGUILayout.BeginVertical();

                //组 选择
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField(L.AssetGroups,GUILayout.MaxWidth(90));
                m_SelectedGroupIndex = EditorGUILayout.Popup(m_SelectedGroupIndex, m_GroupNames!);
                
                if(GUILayout.Button("New", GUILayout.MaxWidth(75)))
                {
                    var index = m_SP_Groups!.arraySize;
                    m_SP_Groups!.InsertArrayElementAtIndex(index);
                    m_SelectedGroupIndex = index;

                    //获取和改名
                    var group = m_SP_Groups!.GetArrayElementAtIndex(index);
                    var groupName = group.FindPropertyRelative("Name");
                    groupName.stringValue = $"Group_{StringHelper.GetRandom(4)}";

                    //清理默认值
                    group.FindPropertyRelative("HideDirectoryStructure").boolValue = false;
                    group.FindPropertyRelative("Patchable").boolValue = true;
                    group.FindPropertyRelative("FolderPaths").ClearArray();
                    group.FindPropertyRelative("AssetPaths").ClearArray();
                    group.FindPropertyRelative("FolderSpecialBuildRules").ClearArray();
                    group.FindPropertyRelative("AssetVariants").ClearArray();
                    group.FindPropertyRelative("FolderVariants").ClearArray();
                }
                if(GUILayout.Button("Delete", GUILayout.MaxWidth(75)))
                {
                    //可否删除
                    var size = m_SP_Groups!.arraySize;
                    if(size <= 1)
                    {
                        EditorUtility.DisplayDialog("Cannot delete group", "The assets package should contain at least one assets group", "Okey");
                        return;
                    }

                    if(EditorUtility.DisplayDialog("Delete?", $"Are you sure you want to delete asset group \"{m_GroupNames![m_SelectedGroupIndex].text}\"?", "Delete", "Cancel"))
                    {
                        m_SP_Groups.DeleteArrayElementAtIndex(m_SelectedGroupIndex);
                        m_SelectedGroupIndex--;
                        if (m_SelectedGroupIndex < 0)
                            m_SelectedGroupIndex = 0;
                    }
                }

                EditorGUILayout.EndHorizontal();

                //------------组 正文------------------
                if(m_SelectedGroupByIndex == null || m_SelectedGroupByIndex.Value != m_SelectedGroupIndex)
                {
                    m_SelectedGroup = m_SP_Groups!.GetArrayElementAtIndex(m_SelectedGroupIndex);
                    m_SelectedGroupByIndex = m_SelectedGroupIndex;
                    RefreshGroupNames();
                }

                EditorGUILayout.Space(15);
                //组名
                EditorGUILayout.PropertyField(m_SelectedGroup!.FindPropertyRelative("Name"), new GUIContent(L.GroupName));
                //隐藏目录结构
                EditorGUILayout.PropertyField(m_SelectedGroup!.FindPropertyRelative("HideDirectoryStructure"), L.HideDirectoryStructure);
                //可用补丁
                EditorGUILayout.PropertyField(m_SelectedGroup!.FindPropertyRelative("Patchable"), L.Patchable);

                EditorGUILayout.EndVertical();

                m_PackageToEdit.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnDestroy()
        {
            if(m_PackageToEdit != null)
            {
                if(_editorWindows.ContainsKey(m_PackageToEdit))
                    _editorWindows.Remove(m_PackageToEdit);
            }

        }

        //------------私有方法--------------------------------------------------------------------

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            if (m_Initialized)
                return;

            if (Styles == null)
                Styles = new StyleDefine();
            if (L == null)
                L = new Localizer();

            if (m_PackageToEdit == null)
                return;



            //检查包
            if (m_PackageToEdit.type.StartsWith("MainPackage"))
            {
                //母包
                titleContent = new GUIContent("VFS-" + L!.EditMainPackage);
            }
            else
            {
                string packageName = m_PackageToEdit.FindPropertyRelative("Name").stringValue;
                titleContent = new GUIContent("VFS-" + string.Format(L!.EditMainPackage, packageName));
            }


            m_SP_Groups = m_PackageToEdit.FindPropertyRelative("Groups");

            RefreshGroupNames();

            m_Initialized = true;
        }

        private void RefreshGroupNames()
        {
            //组名
            var groupNames = new List<GUIContent>(m_SP_Groups!.arraySize);
            for (int i = 0; i < m_SP_Groups.arraySize; i++)
            {
                var group = m_SP_Groups.GetArrayElementAtIndex(i);
                var group_name = group.FindPropertyRelative("Name");
                if (group_name.stringValue.IsNullOrEmpty())
                    groupNames.Add(new GUIContent($"{i + 1} - {L!.EmptyGroupName}"));
                else
                    groupNames.Add(new GUIContent($"{i + 1} - {group_name.stringValue}"));
            }
            m_GroupNames = groupNames.ToArray();
        }

        //------------多语言文本 和 样式-------------------------------------------------------------------------------
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

                Title_Error = new GUIStyle(EditorStyles.largeLabel); //红色
                if (IsHans || IsJp)
                    Title_Error.fontSize += 2;
                else
                    Title_Error.fontSize += 1;

                GroupButton_Normal = new GUIStyle(GUI.skin.button);

                GroupButton_EmptyName = new GUIStyle(GUI.skin.button);
                GroupButton_EmptyName.fontStyle = FontStyle.Italic;

            }

            public GUIStyle Body;

            public GUIStyle Title;

            public GUIStyle Title_Error;

            public GUIStyle GroupButton_Normal;
            public GUIStyle GroupButton_EmptyName;
        }

        class Localizer
        {
            bool IsHans = EditorLocalizationUtil.IsHans();
            bool IsJp = EditorLocalizationUtil.IsJapanese();

            public string EditrObjectMissing
            {
                get
                {
                    if (IsHans)
                        return "编辑对象丢失，请重新打开本窗口";
                    if (IsJp)
                        return "編集オブジェクトが見つかりません、このウィンドウを再度開いてください";
                    return "The edit object is missing, please reopen this window";
                }
            }

            public string EditMainPackage
            {
                get
                {
                    if (IsHans)
                        return "编辑资产主包";
                    return "Edit Assets Main Package";
                }
            }

            public string EditPackage
            {
                get
                {
                    if (IsHans)
                        return "编辑资产包 {0}";
                    return "Edit Assets Package {0}";
                }
            }

            public string AssetGroups
            {
                get
                {
                    if (IsHans)
                        return "资产组";
                    return "Asset Groups";
                }
            }

            

            public string EmptyGroupName
            {
                get
                {
                    if (IsHans)
                        return "空组名";
                    return "Empty Group Name";
                }
            }


            public string GroupName
            {
                get
                {
                    if (IsHans)
                        return "资产组名称";
                    return "Asset Group Name";
                }
            }


            private GUIContent? _HideDirectoryStructure;
            public GUIContent HideDirectoryStructure
            {
                get
                {
                    if (_HideDirectoryStructure == null)
                    {
                        if (IsHans)
                            _HideDirectoryStructure = new GUIContent("隐藏目录结构", "改变AssetBundle的文件结构（默认与项目路径一致）");
                        else
                            _HideDirectoryStructure = new GUIContent("Hide Directory Structure", "Change the file structure of the AssetBundle (default is the same as the project path)");
                    }

                    return _HideDirectoryStructure;
                }
            }

            private GUIContent? _Patchable;
            public GUIContent Patchable
            {
                get
                {
                    if (_Patchable == null)
                    {
                        if (IsHans)
                            _Patchable = new GUIContent("可用补丁", "可使用补丁更新该组的资产");
                        else
                            _Patchable = new GUIContent("Patchable", "Assets in the group can be updated with patches");
                    }

                    return _Patchable;
                }
            }


        }
    }
}
