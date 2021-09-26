using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Const;
using TinaXEditor.Core;

namespace TinaXEditor.VFS.Managers.Config
{
    [UnityEditor.InitializeOnLoad]
    public static class EditorVFSConfigManager
    {
        private static VFSConfigAsset _VFSConfigAsset;

        static EditorVFSConfigManager()
        {

        }

        public static VFSConfigAsset ConfigAsset
        {
            get
            {
                if(_VFSConfigAsset == null)
                {
                    _VFSConfigAsset = EditorConfigAsset.GetConfig<VFSConfigAsset>(VFSConsts.DefaultConfigAssetName);
                }
                return _VFSConfigAsset;
            }
        }

        

    }
}
