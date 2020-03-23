using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaXEditor.VFSKit.Const;
using TinaXEditor.VFSKit.UI;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFSKitInternal
{
    public static class VFSProjectSettng
    {
        [SettingsProvider]
        public static SettingsProvider GetSettings()
        {
            return new SettingsProvider(VFSEditorConst.ProjectSetting_Node, SettingsScope.Project, new string[] { "Nekonya", "TinaX", "VFS", "TinaX.VFS" })
            {
                label = "X VFS",
                guiHandler = (searchContent) =>
                {
                    EditorGUILayout.LabelField("Please open the independent VFS dashboard for relevant settings.");
                    if(GUILayout.Button("Open VFS Dashboard", GUILayout.MaxWidth(160)))
                    {
                        VFSConfigDashboardIMGUI.OpenUI();
                    }
                },
            };
        }
    }
}
