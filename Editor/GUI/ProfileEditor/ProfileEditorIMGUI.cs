using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using TinaX.VFSKit;
using TinaX;
using TinaXEditor;

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
                wnd.minSize = new Vector2(364, 599);
                wnd.maxSize = new Vector2(365, 600);
                Rect pos = wnd.position;
                pos.width = 365;
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


        private string[] xprofiles;
        private int select_xprofile;
        private Vector2 v2_body_scrollview;


        #endregion

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(style_head);
            GUILayout.Label("Profile:",GUILayout.Width(55));
            if(xprofiles == null)
            {
                refreshXprofilesCacheData();
            }

            select_xprofile = EditorGUILayout.Popup(select_xprofile, xprofiles);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(style_body);
            v2_body_scrollview = EditorGUILayout.BeginScrollView(v2_body_scrollview);
            GUILayout.Label("喵");



            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();



            GUILayout.EndVertical();
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

        private void OnDestroy()
        {
            ProfileEditorIMGUI.wnd = null;
        }

    }
}
