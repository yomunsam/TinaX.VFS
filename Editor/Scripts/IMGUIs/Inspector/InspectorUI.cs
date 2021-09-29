using UnityEditor;

namespace TinaXEditor.VFS.IMGUIs.Inspector
{
    [InitializeOnLoad]
    static class InspectorUI
    {
        static InspectorUI()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }


        static void OnPostHeaderGUI(Editor editor)
        {
            EditorGUILayout.LabelField("喵呜");
        }
    }
}
