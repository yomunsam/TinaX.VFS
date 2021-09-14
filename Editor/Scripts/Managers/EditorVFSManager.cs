using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Const;
using TinaXEditor.Core.Utils;
using UnityEditor;

namespace TinaXEditor.VFS.Managers
{
    [InitializeOnLoad]
    public static class EditorVFSManager
    {
        private static VFSConfigAsset m_Config;

        static EditorVFSManager()
        {
            Refresh();
        }


        private static void Refresh()
        {
            m_Config = EditorConfigAssetUtil.GetConfigFromDefaultFolder<VFSConfigAsset>(VFSConst.DefaultConfigAssetName);
            if (m_Config == null)
                return;
        }
    }
}
