using TinaX.VFS.Models;

namespace TinaX.VFS.ConfigTpls
{
    [System.Serializable]
    public class VFSManifestTpl
    {
        public int Version;
        public VFSAssetBundleDetailModel[] Bundles;
    }
}
