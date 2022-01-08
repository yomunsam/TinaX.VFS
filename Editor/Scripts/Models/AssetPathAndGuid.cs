using TinaX.VFS.Models;
using UnityEditor;

#nullable enable
namespace TinaXEditor.VFS.Models
{
    public struct AssetPathAndGuid
    {
        public string AssetPath;
        public string AssetPathLower;
        public GUID GUID;
        public string GuidText;

        public AssetPathAndGuid(string assetPath, GUID guid)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPath.ToLower();
            GUID = guid;
            GuidText = guid.ToString();
        }

        public AssetPathAndGuid(string assetPath, string guid)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPath.ToLower();
            GUID = new GUID(guid);
            GuidText = guid;
        }

        public AssetPathAndGuid(VFSAssetPath assetPath, GUID guid)
        {
            AssetPath = assetPath.AssetPath;
            AssetPathLower = assetPath.AssetPathLower;
            GUID = guid;
            GuidText = guid.ToString();
        }

        public AssetPathAndGuid(VFSAssetPath assetPath, string guid)
        {
            AssetPath = assetPath.AssetPath;
            AssetPathLower = assetPath.AssetPathLower;
            GUID = new GUID(guid);
            GuidText = guid;
        }

        public AssetPathAndGuid(string assetPath, string assetPathLower, string guid)
        {
            AssetPath = assetPath;
            AssetPathLower = assetPathLower;
            GUID = new GUID(guid);
            GuidText = guid;
        }



        public override string ToString()
        {
            return $"{this.AssetPath}({this.GuidText})";
        }
    }
}
