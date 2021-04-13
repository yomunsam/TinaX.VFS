using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.VFSKit.Configuration;
using TinaX.VFSKit.Const;
using UnityEditor;

namespace TinaXEditor.VFSKit.Managers
{
    [InitializeOnLoad]
    public static class EditorVFSManager
    {
        private static VFSConfScriptableObj m_Config;

        static EditorVFSManager()
        {
            Refresh();
        }


        private static void Refresh()
        {
            m_Config = XConfig.GetConfig<VFSConfScriptableObj>(VFSConst.ConfigFilePath_Resources, AssetLoadType.Resources, false);
            if (m_Config == null)
                return;
        }
    }
}
