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

        private string[] groupNames;
        private Dictionary<string, GroupHandleMode> groups_handlemode_cache = new Dictionary<string, GroupHandleMode>(); //string: groupName

        /// <summary>
        /// 编辑缓存 [Profile] -> [Group] -> E_GroupAssetsLocation
        /// </summary>
        private Dictionary<string, Dictionary<string, ProfileRecord.E_GroupAssetsLocation>> assetLocation_cache = new Dictionary<string, Dictionary<string, ProfileRecord.E_GroupAssetsLocation>>();



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
            }
            if(groupNames == null)
            {
                groupNames = VFSManagerEditor.GetGroupNames();
            }

            foreach(var name in groupNames)
            {
                GUILayout.BeginHorizontal();
                //GroupName
                GUILayout.Label(name, style_txt_group_item,GUILayout.Width(170));
                GUILayout.Space(5);
                //资源存储位置
                GroupHandleMode handleMode;
                if (!groups_handlemode_cache.TryGetValue(name,out handleMode))
                {
                    if(!VFSManagerEditor.TryGetGroupHandleMode(name, out handleMode))
                    {
                        Debug.LogError("Get Group Info Failed: " + name) ;
                        this.Close();
                        return;
                    }
                    else
                    {
                        groups_handlemode_cache.Add(name, handleMode);
                    }
                }

                if(handleMode == GroupHandleMode.LocalAndUpdatable || handleMode == GroupHandleMode.LocalOrRemote)
                {
                    //可以主动设置资源位置
                    if (!assetLocation_cache.ContainsKey(xprofiles[select_xprofile]))
                        assetLocation_cache.Add(cur_xprofile_name, new Dictionary<string, ProfileRecord.E_GroupAssetsLocation>());

                    if (!assetLocation_cache[cur_xprofile_name].ContainsKey(name))
                        assetLocation_cache[cur_xprofile_name].Add(name, ProfileRecord.E_GroupAssetsLocation.Local);

                    assetLocation_cache[cur_xprofile_name][name] = (ProfileRecord.E_GroupAssetsLocation)EditorGUILayout.EnumPopup(assetLocation_cache[cur_xprofile_name][name],GUILayout.Width(150));
                }
                else
                {
                    //写死资源位置
                    if (handleMode == GroupHandleMode.LocalOnly)
                    {
                        setAssetLocationCacheValue(cur_xprofile_name, name, ProfileRecord.E_GroupAssetsLocation.Local);
                        GUILayout.Label($"[{ProfileRecord.E_GroupAssetsLocation.Local.ToString()}]", GUILayout.Width(150));
                    }
                    else if (handleMode == GroupHandleMode.RemoteOnly)
                    {
                        setAssetLocationCacheValue(cur_xprofile_name, name, ProfileRecord.E_GroupAssetsLocation.Server);
                        GUILayout.Label($"[{ProfileRecord.E_GroupAssetsLocation.Server.ToString()}]", GUILayout.Width(150));
                    }
                }

                //Disable
                GUILayout.Label("", GUILayout.Width(50));

                GUILayout.EndHorizontal();
            }



            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();



            GUILayout.EndVertical();
        }

        private void OnDisable()
        {
            xprofiles = null;
            mCurProfileRecord = null;
            select_xprofile = 0;
            groups_handlemode_cache?.Clear();
        }

        private void OnLostFocus()
        {
            xprofiles = null;
            mCurProfileRecord = null;
            select_xprofile = 0;
            groups_handlemode_cache?.Clear();
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
            if (!assetLocation_cache.ContainsKey(xprofiles[select_xprofile]))
                assetLocation_cache.Add(profileName, new Dictionary<string, ProfileRecord.E_GroupAssetsLocation>());

            if (assetLocation_cache[profileName].ContainsKey(groupName))
            {
                if (assetLocation_cache[profileName][groupName] != assetsLocation)
                    assetLocation_cache[profileName][groupName] = assetsLocation;
            }
            else
                assetLocation_cache[profileName].Add(groupName, assetsLocation);
        }

        private void OnDestroy()
        {
            ProfileEditorIMGUI.wnd = null;
        }

    }
}
