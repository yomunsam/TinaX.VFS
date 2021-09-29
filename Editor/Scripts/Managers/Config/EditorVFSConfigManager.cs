using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Consts;
using TinaXEditor.Core;

namespace TinaXEditor.VFS.Managers.Config
{
    public static class EditorVFSConfigManager
    {
        private static VFSConfigAsset _VFSConfigAsset;
        
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
