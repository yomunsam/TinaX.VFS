using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Const;
using TinaXEditor.Core;
using TinaXEditor.VFS.ScriptableSingletons;
using UnityEditor;
using UnityEngine;

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
            if (Application.isPlaying)
                return;
            m_Config = EditorConfigAsset.GetConfig<VFSConfigAsset>(VFSConst.DefaultConfigAssetName);
            if (m_Config == null)
                return;

            ScriptableSingleton<EditorVFSOptionScriptableSingleton>.instance.VFSConfigAsset = m_Config;
        }

        
    }
}
