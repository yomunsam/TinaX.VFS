using TinaX.VFS.Scripts.Structs;
using UnityEditor;

namespace TinaXEditor.VFS.AssetBuilder.Structs
{
    public struct AssetPathAndGuid
    {
        public string AssetPath;
        public string AssetPathLower;
        public GUID GUID;

        public AssetPathAndGuid(string assetPath, GUID guid)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPath.ToLower();
            GUID = guid;
        }

        public AssetPathAndGuid(string assetPath, string guid)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPath.ToLower();
            GUID = new GUID(guid);
        }

        public AssetPathAndGuid(VFSAssetPath assetPath, GUID guid)
        {
            AssetPath = assetPath.AssetPath;
            AssetPathLower = assetPath.AssetPathLower;
            GUID = guid;
        }

        public AssetPathAndGuid(VFSAssetPath assetPath, string guid)
        {
            AssetPath = assetPath.AssetPath;
            AssetPathLower = assetPath.AssetPathLower;
            GUID = new GUID(guid);
        }

        public AssetPathAndGuid(string assetPath, string assetPathLower, string guid)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPathLower;
            GUID = new GUID(guid);
        }



        public override string ToString()
        {
            return this.AssetPath;
        }
    }
}
