using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit.FileServer
{
    public static class FileServerEditorInstance
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

        public static int Port { get; set; } = 8080;

        public static bool IsSupported = FileServer.IsSupported;

        public static void StartServer()
        {
            if(mServer == null)
            {
                mServer = new FileServer();
            }
            mServer.Port = Port;
            TinaX.IO.XDirectory.CreateIfNotExists(VFSManagerEditor.GetEditorFileServerRootPath());
            mServer.FilesRootFolder = VFSManagerEditor.GetEditorFileServerRootPath();
            mServer.Run();
        }

        public static void StopServer()
        {
            mServer?.Stop();
        }

    }
}
