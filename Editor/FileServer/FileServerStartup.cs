using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using UnityEditor;

namespace TinaXEditor.VFSKit.FileServer
{
    public class FileServerStartupn : IXBootstrap
    {
        public void OnAppRestart()
        {
            
        }

        public void OnInit()
        {
            if (ScriptableSingleton<FileServerDataCache>.instance.Server_Switch)
            {
                if (!FileServerEditorManager.IsServerRunning)
                    FileServerEditorManager.StartServer();
            }
        }

        public void OnQuit()
        {
            
        }

        public void OnStart()
        {
            
        }
    }
}
