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
        private static FileServer mServer;

        public static bool IsServerRunning
        {
            get
            {
                if (mServer == null) return false;
                return mServer.IsRunning;
            }
        }

        

        

        public static bool IsSupported = FileServer.IsSupported;

        public static void StartServer()
        {
            if(mServer == null)
            {
                mServer = new FileServer();
            }
            mServer.Port = ScriptableSingleton<FileServerDataCache>.instance.Port;
            mServer.FilesRootFolder = ScriptableSingleton<FileServerDataCache>.instance.ServerRootFolder;
            mServer.UrlHead = ScriptableSingleton<FileServerDataCache>.instance.Url;
            mServer.Run();
        }

        public static void StopServer()
        {
            mServer?.Stop();
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
