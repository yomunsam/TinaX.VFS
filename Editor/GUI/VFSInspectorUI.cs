using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace TinaXEditor.VFSKit
{
    [InitializeOnLoad]
    static class VFSInspectorUI
    {
        static VFSInspectorUI()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }


        private static void OnPostHeaderGUI(Editor editor)
        {
            GUILayout.Box("meow");
        }

    }
}

