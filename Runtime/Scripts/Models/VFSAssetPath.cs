namespace TinaX.VFS.Models
{
    public struct VFSAssetPath
    {
        public string AssetPath;
        public string AssetPathLower;

        public VFSAssetPath(string assetPath)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPath.ToLower();
        }
    }
}
