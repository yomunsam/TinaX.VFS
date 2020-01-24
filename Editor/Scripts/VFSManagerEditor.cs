using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TinaX.VFSKit;
using TinaX;
using TinaX.VFSKit.Const;

namespace TinaXEditor.VFSKit
{
    [InitializeOnLoad]
    public static class VFSManagerEditor
    {
        static List<VFSGroup> Groups = new List<VFSGroup>();

        static VFSManagerEditor()
        {
            RefreshManager();
        }

        private static VFSConfigModel mConfig;

        public static void RefreshManager()
        {
            mConfig = XConfig.GetConfig<VFSConfigModel>(VFSConfig.ConfigFilePath_Resources);
            if (mConfig == null) return;

            
        }

        


    }

}

