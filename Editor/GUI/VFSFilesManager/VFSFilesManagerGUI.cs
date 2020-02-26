using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

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
                wnd.maxSize = new Vector2(300, 400);
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }

        string[] vfs_folders;

        private void OnGUI()
        {
        }

        /// <summary>
        /// 准备GUI需要用到的数据
        /// </summary>
        private void RefreshDatas()
        {
            
        }

    }
}
