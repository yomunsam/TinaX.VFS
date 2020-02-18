using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TinaXEditor.VFSKitInternal.I18N;


namespace TinaXEditor.VFSKit.UI
{
    internal class VFSBuilderIMGUI : EditorWindow
    {
        private static VFSBuilderIMGUI wnd;

        //[MenuItem("TinaX/VFS/VFS Dashboard")]
        public static void OpenUI()
        {
            if(wnd == null)
            {
                wnd = GetWindow<VFSBuilderIMGUI>();
                wnd.titleContent = new GUIContent(VFSBuilderI18N.WindowTitle);
                wnd.minSize = new Vector2(364, 599);
                wnd.maxSize = new Vector2(365, 600);
                Rect pos = wnd.position;
                pos.width = 365;
                pos.height = 600;
                wnd.position = pos;
            }
            else
            {
                wnd.Focus();
            }
        }


        private void OnGUI()
        {
        }

    }
}
