using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinaX;
using System.Threading.Tasks;
using UnityEditor;

namespace TinaXEditor.VFSKit.FileServer
{
    public static class FileServerEditorManager
    {
        static FileServerEditorManager()
        {
            if (!EditorApplication.isPlaying)
            {
                if (ScriptableSingleton<FileServerDataCache>.instance.Server_Switch)
                {
                    if (!IsServerRunning)
                    {
                        StartServer();
                    }
                } 
            }
        }

        public static bool IsServerRunning
        {
            get
            {
                if (ScriptableSingleton<FileServerDataCache>.instance.FileServer == null) return false;
                return ScriptableSingleton<FileServerDataCache>.instance.FileServer.IsRunning;
            }
        }
        

        public static bool IsSupported = FileServer.IsSupported;

        public static void StartServer()
        {
            ScriptableSingleton<FileServerDataCache>.instance.Server_Switch = true;
            if (ScriptableSingleton<FileServerDataCache>.instance.FileServer == null)
            {
                ScriptableSingleton<FileServerDataCache>.instance.FileServer = new FileServer();
            }
            ScriptableSingleton<FileServerDataCache>.instance.FileServer.Port = ScriptableSingleton<FileServerDataCache>.instance.Port;
            ScriptableSingleton<FileServerDataCache>.instance.FileServer.FilesRootFolder = ScriptableSingleton<FileServerDataCache>.instance.ServerRootFolder;
            ScriptableSingleton<FileServerDataCache>.instance.FileServer.UrlHead = ScriptableSingleton<FileServerDataCache>.instance.Url;
            ScriptableSingleton<FileServerDataCache>.instance.FileServer.Run();
        }

        public static void StopServer()
        {
            ScriptableSingleton<FileServerDataCache>.instance.Server_Switch = false;
            ScriptableSingleton<FileServerDataCache>.instance.FileServer?.Stop();
        }


        public static void SetRootFolder(string server_root_folder)
        {
            ScriptableSingleton<FileServerDataCache>.instance.ServerRootFolder = server_root_folder;
        }

        public static string GetRootFolder()
        {
            return ScriptableSingleton<FileServerDataCache>.instance.ServerRootFolder;
        }
    }

    public class FileServerDataCache : ScriptableSingleton<FileServerDataCache>
    {
        public bool Server_Switch; //持久记录的一个服务器开关（打开服务器之后，编译或者运行会导致服务器关闭，所以把开关状态记录下来）
        public FileServer FileServer;
        public int Port = 8080;

        public string _url = "/files/";
        public string Url
        {
            get
            {
                if (_url.IsNullOrEmpty() || _url.IsNullOrWhiteSpace()) _url = "/";
                return _url;
            }
            set
            {
                _url = value;
                if (!_url.StartsWith("/"))
                    _url = "/" + _url;

                if (!_url.EndsWith("/"))
                    _url += "/";
            }
        }

        public string ServerRootFolder;
    }
}
