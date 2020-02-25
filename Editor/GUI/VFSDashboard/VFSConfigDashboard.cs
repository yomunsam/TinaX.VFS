// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
// using UnityEditor.UIElements;

// namespace TinaXEditor.VFSKit
// {

//     public class VFSConfigDashboard : EditorWindow
//     {
//         //#if TINAX_DEBUG_DEV
//         //    [MenuItem("Window/UIElements/VFSConfigPlane")]
//         //#endif
//         [MenuItem("TinaX/VFS/VFS Dashboard")]
//         public static void ShowExample()
//         {
//             VFSConfigDashboard wnd = GetWindow<VFSConfigDashboard>();
//             wnd.titleContent = new GUIContent(VFSConfigDashboardI18N.WindowTitle);
//         }

//         const string UXML_Path = @"Packages/io.nekonya.tinax.vfs/Editor/GUI/VFSDashboard/VFSConfigDashboard.uxml";
//         const string USS_Path = "Packages/io.nekonya.tinax.vfs/Editor/GUI/VFSDashboard/VFSConfigDashboard.uss";


//         public void OnEnable()
//         {
//             RefreshGUI();
//         }


// #if TINAX_DEBUG_DEV
//         private void OnGUI()
//         {
//             if (GUILayout.Button("Reload GUI", GUILayout.MaxWidth(80)))
//             {
//                 RefreshGUI();
//             }
//         }

// #endif

//         public void RefreshGUI()
//         {
//             this.rootVisualElement.Clear();

//             //load uxml
//             var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_Path);
//             //load uss
//             var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_Path);
//             this.rootVisualElement.styleSheets.Add(stylesheet);

//             VisualElement elements = visualTree.CloneTree();
//             //elements.styleSheets.Add(stylesheet);
//             rootVisualElement.Add(elements);
//         }
//     }
// }
