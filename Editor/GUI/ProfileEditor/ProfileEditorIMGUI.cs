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
using TinaXEditor.VFSKitInternal;

namespace TinaXEditor.VFSKit.UI
{
    public class ProfileEditorIMGUI : EditorWindow
    {

        static ProfileEditorIMGUI wnd;

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

        private string[] xprofiles;
        private int select_xprofile;
        private Vector2 v2_body_scrollview;
        private ProfileRecord mCurProfileRecord;

        //private string[] groupNames;
        //private Dictionary<string, GroupHandleMode> groups_handlemode_cache = new Dictionary<string, GroupHandleMode>(); //string: groupName
        private List<VFSGroup> groups;

        /// <summary>
        /// 编辑缓存 [Profile] -> [Group] -> E_GroupAssetsLocation
        /// </summary>
        private Dictionary<string, Dictionary<string, ProfileRecord.E_GroupAssetsLocation>> assetLocation_cache = new Dictionary<string, Dictionary<string, ProfileRecord.E_GroupAssetsLocation>>();

        /// <summary>
        /// 编辑缓存：Group是否可以disable [Profile] -> [Group] -> E_GroupAssetsLocation
        /// </summary>
        private Dictionary<string, Dictionary<string, bool>> disable_expansionGroup_cache = new Dictionary<string, Dictionary<string, bool>>();

        #endregion

        private void OnEnable()
        {
            if(XCoreEditor.GetXProfiles().Length == 0)
            {
                XCoreEditor.RefreshXProfile();
            }
            refreshXprofilesCacheData();

        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(style_head);
            GUILayout.Label("Profile:",GUILayout.Width(55));
            if(xprofiles == null || (select_xprofile -1) > xprofiles.Length)
            {
                refreshXprofilesCacheData();
            }

            select_xprofile = EditorGUILayout.Popup(select_xprofile, xprofiles);
            GUILayout.EndHorizontal();

            #region Group列表

            GUILayout.BeginVertical(style_body);
            v2_body_scrollview = EditorGUILayout.BeginScrollView(v2_body_scrollview);

            string cur_xprofile_name = xprofiles[select_xprofile];

            //表头
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Group",GUILayout.Width(165));
            GUILayout.Label("|", GUILayout.Width(10));
            GUILayout.Label(IsChinese ? "资源存储位置" : "Assets Storage Location",GUILayout.Width(150));
            GUILayout.Label("|",GUILayout.Width(10));
            GUILayout.Label("Disable",GUILayout.Width(50));

            EditorGUILayout.EndHorizontal();

            if(mCurProfileRecord == null || xprofiles[select_xprofile] != mCurProfileRecord.ProfileName)
            {
                mCurProfileRecord = VFSManagerEditor.GetProfileRecord(xprofiles[select_xprofile]);
                addConfiguredValueToCache();
            }
            if(groups == null)
            {
                groups = VFSManagerEditor.GetGroups();
                //groupNames = VFSManagerEditor.GetGroupNames();
            }

            foreach(var group in groups)
            {
                GUILayout.BeginHorizontal();
                //GroupName
                GUILayout.Label(group.GroupName, style_txt_group_item,GUILayout.Width(170));
                GUILayout.Space(5);
                //资源存储位置
                GroupHandleMode handleMode = group.HandleMode;
                

                if(handleMode == GroupHandleMode.LocalAndUpdatable || handleMode == GroupHandleMode.LocalOrRemote)
                {
                    //可以主动设置资源位置
                    if (!assetLocation_cache.ContainsKey(cur_xprofile_name))
                        assetLocation_cache.Add(cur_xprofile_name, new Dictionary<string, ProfileRecord.E_GroupAssetsLocation>());

                    if (!assetLocation_cache[cur_xprofile_name].ContainsKey(group.GroupName))
                        assetLocation_cache[cur_xprofile_name].Add(group.GroupName, ProfileRecord.E_GroupAssetsLocation.Local);

                    assetLocation_cache[cur_xprofile_name][group.GroupName] = (ProfileRecord.E_GroupAssetsLocation)EditorGUILayout.EnumPopup(assetLocation_cache[cur_xprofile_name][group.GroupName],GUILayout.Width(150));

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
                        setAssetLocationCacheValue(cur_xprofile_name, group.GroupName, ProfileRecord.E_GroupAssetsLocation.Server);
                        GUILayout.Label($"[{ProfileRecord.E_GroupAssetsLocation.Server.ToString()}]", GUILayout.Width(150));
                    }
                }

                GUILayout.Space(10);
                //Disable
                if (group.ExpansionGroup)
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
                    GUILayout.Label("[-]", style_label_center,GUILayout.Width(36));
                    setGroupDisableCacheValue(cur_xprofile_name, group.GroupName, false);
                }

                GUILayout.EndHorizontal();
            }



            EditorGUILayout.EndScrollView();

            

            GUILayout.EndVertical();
            #endregion

            #region 保存和重置值的两个按钮
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            //包存cache的值
            if(GUILayout.Button(IsChinese ? "保存设置" : "Save", GUILayout.Width(100)))
            {
                foreach(var item in assetLocation_cache)
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

                //通知VFSManager保存到disk
                VFSManagerEditor.SaveProfileRecord();
            }

            //重置cache的已记录值
            if(GUILayout.Button(IsChinese ? "重置设置" : "Reset modify", GUILayout.Width(100)))
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
            //groups_handlemode_cache?.Clear();
        }

        private void OnLostFocus()
        {
            xprofiles = null;
            mCurProfileRecord = null;
            select_xprofile = 0;
            groups = null;
            //groups_handlemode_cache?.Clear();
        }

        void refreshXprofilesCacheData()
        {
            xprofiles = XCoreEditor.GetXProfiles();
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
            }
            
        }

        private void OnDestroy()
        {
            ProfileEditorIMGUI.wnd = null;
        }

    }
}
