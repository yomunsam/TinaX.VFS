using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using TinaX.VFSKit;
using TinaX;
using TinaX.Internal;
using TinaXEditor;
using TinaXEditor.VFSKit;
using TinaXEditor.VFSKitInternal;
using UnityEditorInternal;

namespace TinaXEditor.VFSKit.UI
{
    public class ProfileEditorIMGUI : EditorWindow
    {

        static ProfileEditorIMGUI wnd;

        public static int? param_toolbar_index;

        public static void OpenUI()
        {
            if(wnd == null)
            {
                wnd = GetWindow<ProfileEditorIMGUI>();
                wnd.titleContent = new GUIContent("VFS Profile");
                wnd.minSize = new Vector2(424, 599);
                wnd.maxSize = new Vector2(425, 600);
                Rect pos = wnd.position;
                pos.width = 425;
                pos.height = 600;
                wnd.position = pos;
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }


        #region Styles
        private GUIStyle _style_head;
        private GUIStyle style_head
        {
            get
            {
                if(_style_head == null)
                {
                    _style_head = new GUIStyle();
                    _style_head.margin.left = 15;
                    _style_head.margin.right = 15;
                    _style_head.padding.top = 25;
                }
                return _style_head;
            }
        }

        private GUIStyle _style_body;
        private GUIStyle style_body
        {
            get
            {
                if (_style_body == null)
                {
                    _style_body = new GUIStyle(EditorStyles.helpBox);
                    _style_body.margin.top = 10;
                }
                return _style_body;
            }
        }

        private GUIStyle _style_label_center;
        private GUIStyle style_label_center
        {
            get
            {
                if (_style_label_center == null)
                {
                    _style_label_center = new GUIStyle(EditorStyles.label);
                    _style_label_center.alignment = TextAnchor.MiddleCenter;
                }
                return _style_label_center;
            }
        }

        


        private static GUIStyle _style_txt_group_item;
        private static GUIStyle style_txt_group_item
        {
            get
            {
                if (_style_txt_group_item == null)
                {
                    _style_txt_group_item = new GUIStyle(EditorStyles.label);
                    _style_txt_group_item.normal.textColor = XEditorColorDefine.Color_Normal_Pure;
                    _style_txt_group_item.fontSize = 13;
                    _style_txt_group_item.fontStyle = FontStyle.BoldAndItalic;
                    _style_txt_group_item.padding.left = 5;
                }
                return _style_txt_group_item;
            }
        }

        #endregion

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

        private string[] _toolbar_str;
        private string[] toolbar_str
        {
            get
            {
                if(_toolbar_str == null)
                {
                    if (IsChinese)
                    {
                        _toolbar_str = new string[] { "文件组", "WebVFS" };
                    }
                    else
                    {
                        _toolbar_str = new string[] { "Groups", "WebVFS" };
                    }
                }
                return _toolbar_str;
            }
        }
        private int mToolbar_Index;

        private string[] xprofiles;
        private int select_xprofile;
        private Vector2 v2_body_groups_scrollview;
        private Vector2 v2_body_webvfs_scrollview;
        private ProfileRecord mCurProfileRecord;


        //private string[] groupNames;
        //private Dictionary<string, GroupHandleMode> groups_handlemode_cache = new Dictionary<string, GroupHandleMode>(); //string: groupName
        private List<VFSEditorGroup> groups;

        /// <summary>
        /// 编辑缓存 [Profile] -> [Group] -> E_GroupAssetsLocation
        /// </summary>
        private Dictionary<string, Dictionary<string, ProfileRecord.E_GroupAssetsLocation>> assetLocation_cache = new Dictionary<string, Dictionary<string, ProfileRecord.E_GroupAssetsLocation>>();

        /// <summary>
        /// 编辑缓存：Group是否可以disable [Profile] -> [Group] -> E_GroupAssetsLocation
        /// </summary>
        private Dictionary<string, Dictionary<string, bool>> disable_expansionGroup_cache = new Dictionary<string, Dictionary<string, bool>>();

        private WebVFSNetworkConfig mWebVFS_Net_Config;
        private SerializedObject mWebVFS_Net_Config_serialized;
        private string mCur_WebVFS_profileNmae;
        private int mCur_ProfileIndex_InWebVFS = 0;
        private ReorderableList reorderableList_urls;

        //private Dictionary<string, WebVFSNetworkConfig.NetworkConfig> mDict_NetworkConfigs = new Dictionary<string, WebVFSNetworkConfig.NetworkConfig>();
        //private Dictionary<string, SerializedProperty> mDict_NetworkConfigs_SerializedProperty = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            if(XCoreEditor.GetXProfileNames().Length == 0)
            {
                XCoreEditor.RefreshXProfile();
            }
            refreshXprofilesCacheData();

            if(param_toolbar_index != null && param_toolbar_index.Value < toolbar_str.Length)
            {
                mToolbar_Index = param_toolbar_index.Value;
                param_toolbar_index = null;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(style_head);
            GUILayout.Label("Profile:",GUILayout.Width(55));


            if (xprofiles == null || (select_xprofile -1) > xprofiles.Length)
            {
                refreshXprofilesCacheData();
            }

            select_xprofile = EditorGUILayout.Popup(select_xprofile, xprofiles);
            GUILayout.EndHorizontal();
            
            //数据
            string cur_xprofile_name = xprofiles[select_xprofile];
            if (mCurProfileRecord == null || xprofiles[select_xprofile] != mCurProfileRecord.ProfileName)
            {
                mCurProfileRecord = VFSManagerEditor.GetProfileRecord(xprofiles[select_xprofile]);
                addConfiguredValueToCache();
            }

            // 工具栏
            GUILayout.Space(5);
            mToolbar_Index = GUILayout.Toolbar(mToolbar_Index, toolbar_str);

            if (mToolbar_Index == 0)
            {
                #region Group列表

                GUILayout.BeginVertical(style_body);
                v2_body_groups_scrollview = EditorGUILayout.BeginScrollView(v2_body_groups_scrollview);


                //表头
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Group", GUILayout.Width(165));
                GUILayout.Label("|", GUILayout.Width(10));
                GUILayout.Label(IsChinese ? "资源存储位置" : "Assets Storage Location", GUILayout.Width(150));
                GUILayout.Label("|", GUILayout.Width(10));
                GUILayout.Label("Disable", GUILayout.Width(50));

                EditorGUILayout.EndHorizontal();


                if (groups == null)
                {
                    groups = VFSManagerEditor.GetGroups();
                    //groupNames = VFSManagerEditor.GetGroupNames();
                }

                foreach (var group in groups)
                {
                    GUILayout.BeginHorizontal();
                    //GroupName
                    GUILayout.Label(group.GroupName, style_txt_group_item, GUILayout.Width(170));
                    GUILayout.Space(5);
                    //资源存储位置
                    GroupHandleMode handleMode = group.HandleMode;


                    if (handleMode == GroupHandleMode.LocalAndUpdatable || handleMode == GroupHandleMode.LocalOrRemote)
                    {
                        //可以主动设置资源位置
                        if (!assetLocation_cache.ContainsKey(cur_xprofile_name))
                            assetLocation_cache.Add(cur_xprofile_name, new Dictionary<string, ProfileRecord.E_GroupAssetsLocation>());

                        if (!assetLocation_cache[cur_xprofile_name].ContainsKey(group.GroupName))
                            assetLocation_cache[cur_xprofile_name].Add(group.GroupName, ProfileRecord.E_GroupAssetsLocation.Local);

                        assetLocation_cache[cur_xprofile_name][group.GroupName] = (ProfileRecord.E_GroupAssetsLocation)EditorGUILayout.EnumPopup(assetLocation_cache[cur_xprofile_name][group.GroupName], GUILayout.Width(150));

                        GUILayout.Space(2);
                    }
                    else
                    {
                        //写死资源位置
                        if (handleMode == GroupHandleMode.LocalOnly)
                        {
                            setAssetLocationCacheValue(cur_xprofile_name, group.GroupName, ProfileRecord.E_GroupAssetsLocation.Local);
                            GUILayout.Label($"[{ProfileRecord.E_GroupAssetsLocation.Local.ToString()}]", GUILayout.Width(150));
                        }
                        else if (handleMode == GroupHandleMode.RemoteOnly)
                        {
                            setAssetLocationCacheValue(cur_xprofile_name, group.GroupName, ProfileRecord.E_GroupAssetsLocation.Remote);
                            GUILayout.Label($"[{ProfileRecord.E_GroupAssetsLocation.Remote.ToString()}]", GUILayout.Width(150));
                        }
                    }

                    GUILayout.Space(10);
                    //Disable
                    if (group.ExtensionGroup)
                    {
                        //可以主动设置group disable
                        if (!disable_expansionGroup_cache.ContainsKey(cur_xprofile_name))
                            disable_expansionGroup_cache.Add(cur_xprofile_name, new Dictionary<string, bool>());

                        if (!disable_expansionGroup_cache[cur_xprofile_name].ContainsKey(group.GroupName))
                            disable_expansionGroup_cache[cur_xprofile_name].Add(group.GroupName, false);
                        GUILayout.Space(10);
                        disable_expansionGroup_cache[cur_xprofile_name][group.GroupName] = EditorGUILayout.Toggle(disable_expansionGroup_cache[cur_xprofile_name][group.GroupName], GUILayout.Width(50));
                    }
                    else
                    {
                        GUILayout.Label("[-]", style_label_center, GUILayout.Width(36));
                        setGroupDisableCacheValue(cur_xprofile_name, group.GroupName, false);
                    }

                    GUILayout.EndHorizontal();
                }



                EditorGUILayout.EndScrollView();



                GUILayout.EndVertical();
                #endregion
            }
            else if (mToolbar_Index == 1)
            {
                #region WebVFS URLs

                if (mWebVFS_Net_Config == null)
                {
                    mWebVFS_Net_Config = XConfig.CreateConfigIfNotExists<WebVFSNetworkConfig>(TinaX.VFSKit.Const.VFSConst.Config_WebVFS_URLs);
                }
                if (mWebVFS_Net_Config_serialized == null)
                {
                    mWebVFS_Net_Config_serialized = new SerializedObject(mWebVFS_Net_Config);
                }
                if (mCur_WebVFS_profileNmae == null || mCur_WebVFS_profileNmae != cur_xprofile_name)
                {
                    if (mWebVFS_Net_Config.Configs == null || !mWebVFS_Net_Config.Configs.Any(item => item.ProfileName == cur_xprofile_name))
                    {
                        ArrayUtil.Combine(ref mWebVFS_Net_Config.Configs, new WebVFSNetworkConfig.NetworkConfig[] {
                            new WebVFSNetworkConfig.NetworkConfig()
                            {
                                ProfileName = cur_xprofile_name
                            }
                        });
                        mWebVFS_Net_Config_serialized.UpdateIfRequiredOrScript();
                    }
                    for(int i = 0; i < mWebVFS_Net_Config.Configs.Length; i++)
                    {
                        if(mWebVFS_Net_Config.Configs[i].ProfileName == cur_xprofile_name)
                        {
                            mCur_ProfileIndex_InWebVFS = i;
                        }
                    }
                    SerializedProperty cur_config = mWebVFS_Net_Config_serialized.FindProperty("Configs").GetArrayElementAtIndex(mCur_ProfileIndex_InWebVFS);
                    SerializedProperty cur_urls = cur_config.FindPropertyRelative("Urls");

                    reorderableList_urls = new ReorderableList(
                                                mWebVFS_Net_Config_serialized,
                                                cur_urls,
                                                true,
                                                true,
                                                true,
                                                true);
                    reorderableList_urls.drawElementCallback = (rect, index, selected, focused) =>
                    {
                        SerializedProperty itemData = reorderableList_urls.serializedProperty.GetArrayElementAtIndex(index);
                        SerializedProperty mode = itemData.FindPropertyRelative("NetworkMode");
                        SerializedProperty url = itemData.FindPropertyRelative("BaseUrl");
                        SerializedProperty helloUrl = itemData.FindPropertyRelative("HelloUrl");

                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight * 1;

                        var rect_mode = rect;
                        EditorGUI.PropertyField(rect_mode, mode, new GUIContent(IsChinese ? "网络模式" : "Network Mode"));
                        //--------------------------------------------------------------------------------------------
                        var rect_url_label = rect;
                        rect_url_label.y += EditorGUIUtility.singleLineHeight + 2;
                        rect_url_label.width = 58;
                        GUI.Label(rect_url_label, IsChinese ? "基础Url" : "Base Url");

                        var rect_url = rect;
                        rect_url.y += EditorGUIUtility.singleLineHeight + 2;
                        rect_url.width -= 62;
                        rect_url.x += 62;
                        EditorGUI.PropertyField(rect_url, url, GUIContent.none);
                        //---------------------------------------------------------------------------------------------------

                        var rect_hellourl_label = rect;
                        rect_hellourl_label.y += EditorGUIUtility.singleLineHeight * 2 + 4;
                        rect_hellourl_label.width = 58;
                        GUI.Label(rect_hellourl_label, IsChinese ? "HelloUrl" : "Hello Url");

                        var rect_helloUrl = rect;
                        rect_helloUrl.y += EditorGUIUtility.singleLineHeight * 2 + 4;
                        rect_helloUrl.width -= 62;
                        rect_helloUrl.x += 62;
                        EditorGUI.PropertyField(rect_helloUrl, helloUrl, GUIContent.none);

                    };
                    reorderableList_urls.elementHeightCallback = (index) =>
                    {
                        return EditorGUIUtility.singleLineHeight * 3 + 10;
                    };
                    reorderableList_urls.drawHeaderCallback = (rect) =>
                    {
                        GUI.Label(rect, IsChinese?"Web Urls":"Web Urls");
                    };

                    mCur_WebVFS_profileNmae = cur_xprofile_name;
                }

                GUILayout.BeginVertical(style_body);
                v2_body_webvfs_scrollview = EditorGUILayout.BeginScrollView(v2_body_webvfs_scrollview);

                reorderableList_urls.DoLayoutList();
                //EditorGUILayout.PropertyField(cur_urls);

                EditorGUILayout.EndScrollView();

                GUILayout.EndVertical();
                #endregion
            }

            #region 保存和重置值的两个按钮
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            //包存cache的值
            if(GUILayout.Button(IsChinese ? "保存设置" : "Save", GUILayout.Width(100)))
            {
                #region Group Profile
                foreach (var item in assetLocation_cache)
                {
                    //item.key: profile,
                    var profile = VFSManagerEditor.GetProfileRecord(item.Key); //因为获取到的是class，所以直接修改它就行了
                    //item.value 是一个字典，key = groupName, value = value
                    foreach(var item2 in item.Value)
                    {
                        //接下来要在结构体上操作了，所以不能拉过来操作了,只能找到下标之后直接去修改
                        if(!profile.GroupProfileRecords.Any(gpr => gpr.GroupName == item2.Key))
                        {
                            ArrayUtil.Combine(ref profile.GroupProfileRecords, new ProfileRecord.S_GroupProfileRecord[]
                            {
                                new ProfileRecord.S_GroupProfileRecord()
                                {
                                    GroupName = item2.Key,
                                }
                            });
                        }
                        
                        for(var i = 0; i < profile.GroupProfileRecords.Length; i++)
                        {
                            if(profile.GroupProfileRecords[i].GroupName == item2.Key)
                            {
                                profile.GroupProfileRecords[i].Location = item2.Value;
                                break;
                            }
                        }

                    }

                    VFSManagerEditor.AddProfileIfNotExists(profile);
                }

                foreach (var item in disable_expansionGroup_cache)
                {
                    //item.key: profile,
                    var profile = VFSManagerEditor.GetProfileRecord(item.Key); //因为获取到的是class，所以直接修改它就行了
                    //item.value 是一个字典，key = groupName, value = value
                    foreach (var item2 in item.Value)
                    {
                        //接下来要在结构体上操作了，所以不能拉过来操作了,只能找到下标之后直接去修改
                        if (!profile.GroupProfileRecords.Any(gpr => gpr.GroupName == item2.Key))
                        {
                            ArrayUtil.Combine(ref profile.GroupProfileRecords, new ProfileRecord.S_GroupProfileRecord[]
                            {
                                new ProfileRecord.S_GroupProfileRecord()
                                {
                                    GroupName = item2.Key,
                                }
                            });
                        }

                        for (var i = 0; i < profile.GroupProfileRecords.Length; i++)
                        {
                            if (profile.GroupProfileRecords[i].GroupName == item2.Key)
                            {
                                profile.GroupProfileRecords[i].DisableGroup = item2.Value;
                                break;
                            }
                        }

                    }

                    VFSManagerEditor.AddProfileIfNotExists(profile);
                }

                //foreach (var item in developMode_cache)
                //{
                //    //item.key: profile,
                //    var profile = VFSManagerEditor.GetProfileRecord(item.Key); //因为获取到的是class，所以直接修改它就行了
                //    //item.value 是 是否为开发模式的value
                //    profile.DevelopMode = item.Value;
                //    VFSManagerEditor.AddProfileIfNotExists(profile);
                //}


                //通知VFSManager保存到disk
                VFSManagerEditor.SaveProfileRecord();
                #endregion

                #region WebVFS URL
                mWebVFS_Net_Config_serialized?.ApplyModifiedPropertiesWithoutUndo();
                //删掉可能多余的内容
                string[] profiles = XCoreEditor.GetXProfileNames();
                if(mWebVFS_Net_Config.Configs != null && mWebVFS_Net_Config.Configs.Length > 0)
                {
                    List<WebVFSNetworkConfig.NetworkConfig> list_temp = new List<WebVFSNetworkConfig.NetworkConfig>(mWebVFS_Net_Config.Configs);
                    for(var i = list_temp.Count - 1; i >= 0; i--)
                    {
                        if (!profiles.Contains(list_temp[i].ProfileName))
                        {
                            list_temp.RemoveAt(i);
                        }
                    }
                    if(list_temp.Count != mWebVFS_Net_Config.Configs.Length)
                    {
                        mWebVFS_Net_Config.Configs = list_temp.ToArray();
                    }
                }

                AssetDatabase.SaveAssets();
                #endregion
            }

            //重置cache的已记录值
            if (GUILayout.Button(IsChinese ? "重置设置" : "Reset modify", GUILayout.Width(100)))
            {
                assetLocation_cache?.Clear();
                disable_expansionGroup_cache?.Clear();
                addConfiguredValueToCache();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            #endregion 
            GUILayout.Space(5);

            GUILayout.EndVertical();
        }

        private void OnDisable()
        {
            xprofiles = null;
            mCurProfileRecord = null;
            select_xprofile = 0;
            groups = null;
            mCur_WebVFS_profileNmae = null;
            //groups_handlemode_cache?.Clear();
        }

        private void OnLostFocus()
        {
            xprofiles = null;
            mCurProfileRecord = null;
            select_xprofile = 0;
            groups = null;
            mCur_WebVFS_profileNmae = null;
            //groups_handlemode_cache?.Clear();
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

        private void setAssetLocationCacheValue(string profileName ,string groupName, ProfileRecord.E_GroupAssetsLocation assetsLocation)
        {
            if (!assetLocation_cache.ContainsKey(profileName))
                assetLocation_cache.Add(profileName, new Dictionary<string, ProfileRecord.E_GroupAssetsLocation>());

            if (assetLocation_cache[profileName].ContainsKey(groupName))
            {
                if (assetLocation_cache[profileName][groupName] != assetsLocation)
                    assetLocation_cache[profileName][groupName] = assetsLocation;
            }
            else
                assetLocation_cache[profileName].Add(groupName, assetsLocation);
        }

        private void setGroupDisableCacheValue(string profileName, string groupName, bool diable)
        {
            if (!disable_expansionGroup_cache.ContainsKey(profileName))
                disable_expansionGroup_cache.Add(profileName, new Dictionary<string, bool>());

            if (disable_expansionGroup_cache[profileName].ContainsKey(groupName))
            {
                if (disable_expansionGroup_cache[profileName][groupName] != diable)
                    disable_expansionGroup_cache[profileName][groupName] = diable;
            }
            else
                disable_expansionGroup_cache[profileName].Add(groupName, diable);
        }

        /// <summary>
        /// 仅当字典中没有这个key的情况下才赋值
        /// </summary>
        /// <param name="profileName"></param>
        /// <param name="groupName"></param>
        /// <param name="assetsLocation"></param>
        private void setAssetLocationCacheIfNoValue(string profileName, string groupName, ProfileRecord.E_GroupAssetsLocation assetsLocation)
        {
            if (!assetLocation_cache.ContainsKey(profileName))
                assetLocation_cache.Add(profileName, new Dictionary<string, ProfileRecord.E_GroupAssetsLocation>());

            if (!assetLocation_cache[profileName].ContainsKey(groupName))
            {
                assetLocation_cache[profileName].Add(groupName, assetsLocation);
            }
        }

        private void setGroupDisableCacheIfNoValue(string profileName, string groupName, bool diable)
        {
            if (!disable_expansionGroup_cache.ContainsKey(profileName))
                disable_expansionGroup_cache.Add(profileName, new Dictionary<string, bool>());

            if (!disable_expansionGroup_cache[profileName].ContainsKey(groupName))
            {
                disable_expansionGroup_cache[profileName].Add(groupName, diable);
            }
        }

        

        /// <summary>
        /// 将已有的配置值存进cache
        /// </summary>
        private void addConfiguredValueToCache()
        {
            if(mCurProfileRecord != null)
            {
                foreach (var item in mCurProfileRecord.GroupProfileRecords)
                {
                    setAssetLocationCacheIfNoValue(mCurProfileRecord.ProfileName, item.GroupName, item.Location);
                    setGroupDisableCacheIfNoValue(mCurProfileRecord.ProfileName, item.GroupName, item.DisableGroup);
                }
                //setDevelopModeCacheIfNoValue(mCurProfileRecord.ProfileName, mCurProfileRecord.DevelopMode);
            }
            
        }

        private void OnDestroy()
        {
            ProfileEditorIMGUI.wnd = null;
        }

    }
}
