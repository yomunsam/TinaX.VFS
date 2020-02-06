using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TinaXEditor.VFSKit.Utils;
using TinaX.VFSKit.Const;
using System;
using System.Linq;

namespace TinaXEditor.VFSKit
{
    using Object = UnityEngine.Object;

    [InitializeOnLoad]
    static class VFSInspectorUI
    {
        static VFSInspectorUI()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        private static string mCurShowAssetPath;
        private static string mCurSelectedAssetPath;

        private static void OnPostHeaderGUI(Editor editor)
        {
            if(editor.targets.Length == 1)
            {
                string guid = string.Empty;
                if(VFSEditorUtil.GetPathAndGUIDFromTarget(editor.target,out string path,ref guid, out Type mainAssetType))
                {
                    mCurSelectedAssetPath = path;
                    if(mCurSelectedAssetPath != mCurShowAssetPath)
                    {
                        //获取数据
                    }

                    if (!IsTypeIgnore(mainAssetType))
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.Space();
                        GUILayout.Label("VFS Asset:");
                        EditorGUILayout.Separator();

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }

        private static bool IsTypeIgnore(Type type)
        {
            return VFSConst.IgnoreType.Any(t => t == type);
        }

    }
}

