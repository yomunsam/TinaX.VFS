using TinaX.VFSKit;

namespace TinaX.VFSKitInternal
{
    public struct AssetQueryResult
    {
        public string AssetPath { get; set; }
        public string AssetPathLower { get; set; }
        public string AssetExtensionName { get; set; } //资源扩展名
        public bool Vliad { get; set; } //资源是否有效
        public string GroupName { get; set; } //组名
        public bool ExtensionGroup { get; set; }
        public string AssetBundleName { get; set; }
        public string AssetBundleNameWithoutExtension { get; set; } //无后缀AssetBundle名
        public FolderBuildDevelopType DevelopType { get; set; }
        public FolderBuildType BuildType { get; set; }
        public GroupHandleMode GroupHandleMode { get; set; }

    }
}
