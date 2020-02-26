using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFSKit.FileServer
{
    internal class FileServerGUI : EditorWindow
    {
        static FileServerGUI wnd;
        public static void OpenUI()
        {
            if(wnd == null)
            {
                wnd = GetWindow<FileServerGUI>();
                wnd.titleContent = new GUIContent("VFS File Server");
                wnd.maxSize = new Vector2(300, 400);
            }
            else
            {
                wnd.Show();
                wnd.Focus();
            }
        }





        private void OnDestroy()
        {
            wnd = null;
        }


        private void OnGUI()
        {
            //Port
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Listen Port:");
            FileServerEditorInstance.Port = EditorGUILayout.IntField(FileServerEditorInstance.Port);
            EditorGUILayout.EndHorizontal();
            #region switch
            if (FileServerEditorInstance.IsServerRunning)
            {
                if (GUILayout.Button(IsChinese ? "关闭服务器" : "Stop Server"))
                {
                    FileServerEditorInstance.StopServer();
                }
            }
            else
            {
                if (GUILayout.Button(IsChinese ? "启动服务器" : "Start Server"))
                {
                    FileServerEditorInstance.StartServer();
                }
            }
            #endregion
            EditorGUILayout.HelpBox(IsChinese?"请仅将该文件服务器用于本机和内网的调试用途。": "Please use this file server only for debugging this computer and the internal network.", MessageType.Info);
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
    }
}
