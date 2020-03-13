using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Net;
using TinaX;
using TinaXEditor.VFSKit.Utils;
using TinaXEditor.VFSKit.Const;

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

        private bool get_local_ip = false;
        private string local_ip;



        private void OnDestroy()
        {
            wnd = null;
        }


        private void OnGUI()
        {
            GUILayout.Space(15);
            if (!FileServerEditorManager.IsServerRunning)
            {
                //Port
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Listen Port:");
                ScriptableSingleton<FileServerDataCache>.instance.Port = EditorGUILayout.IntField(ScriptableSingleton<FileServerDataCache>.instance.Port);

                EditorGUILayout.EndHorizontal();

                //Url 头
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Url Start:");
                ScriptableSingleton<FileServerDataCache>.instance.Url = EditorGUILayout.TextField(ScriptableSingleton<FileServerDataCache>.instance.Url);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            #region switch
            if (FileServerEditorManager.IsServerRunning)
            {
                if (GUILayout.Button(IsChinese ? "关闭服务器" : "Stop Server"))
                {
                    FileServerEditorManager.StopServer();
                }
            }
            else
            {
                if (GUILayout.Button(IsChinese ? "启动服务器" : "Start Server"))
                {
                    FileServerEditorManager.StartServer();
                }
            }
            #endregion
            EditorGUILayout.HelpBox(IsChinese?"请仅将该文件服务器用于本机和内网的调试用途。": "Please use this file server only for debugging this computer and the internal network.", MessageType.Info);
            
            EditorGUILayout.Space();
            #region 显示Url地址
            //ip
            GUILayout.Label("Address");
            string localhost_addr = $"http://127.0.0.1{(ScriptableSingleton<FileServerDataCache>.instance.Port == 80 ? "" : ":" + ScriptableSingleton<FileServerDataCache>.instance.Port)}{ScriptableSingleton<FileServerDataCache>.instance.Url}";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(localhost_addr, EditorStyles.linkLabel);
            if (GUILayout.Button("Copy",GUILayout.Width(50)))
            {
                GUIUtility.systemCopyBuffer = localhost_addr;
            }
            EditorGUILayout.EndHorizontal();
            if (!get_local_ip) TryGetLocalIp(out local_ip);
            if(!local_ip.IsNullOrEmpty())
            {
                EditorGUILayout.BeginHorizontal();
                string lan_ip = $"http://{local_ip}{(ScriptableSingleton<FileServerDataCache>.instance.Port == 80 ? "" : ":" + ScriptableSingleton<FileServerDataCache>.instance.Port)}{ScriptableSingleton<FileServerDataCache>.instance.Url}";
                EditorGUILayout.LabelField(lan_ip, EditorStyles.linkLabel);
                if (GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    GUIUtility.systemCopyBuffer = lan_ip;
                }
                EditorGUILayout.EndHorizontal();
            }

            #endregion

            #region Server Root Folder
            EditorGUILayout.Space();
            GUILayout.Label("WWWRoot:");
            EditorGUILayout.TextArea(ScriptableSingleton<FileServerDataCache>.instance.ServerRootFolder);
            if(GUILayout.Button(IsChinese?"设置目录":"Set wwwroot folder"))
            {
                string source_package_root = VFSEditorConst.PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent(IsChinese ? "资源构建目录" : "Built Asset Folder"),
                    (FileServerEditorManager.GetRootFolder() == source_package_root),
                    () => {
                        FileServerEditorManager.SetRootFolder(source_package_root);
                    });

                menu.ShowAsContext();
            }
            #endregion
        }

        private bool TryGetLocalIp(out string ip)
        {
            try
            {
                var host_name = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(host_name);
                foreach(var item in ipEntry.AddressList)
                {
                    if(item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = item.ToString();
                        return true;
                    }
                }
            }
            catch
            {
                
            }
            ip = null;
            return false;
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
