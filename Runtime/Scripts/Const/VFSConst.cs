namespace TinaX.VFSKit.Const
{
    public static class VFSConst
    {
        public const string ServiceName = "TinaX.VFS";

        public const string ConfigFileName = "VFSConfig";

        public static string ConfigFilePath_Resources = $"{TinaX.Const.FrameworkConst.Framework_Configs_Folder_Path}/{ConfigFileName}";

        public static string Config_WebVFS_URLs = $"{TinaX.Const.FrameworkConst.Framework_Configs_Folder_Path}/WebVFSUrl";

        public static System.Type[] IgnoreType =
        {
#if UNITY_EDITOR
            typeof(UnityEditor.MonoScript),
            typeof(UnityEditor.DefaultAsset),
#endif
        };

        public const string AssetBundleFilesHash_FileName = "FilesHash.json";

        /// <summary>
        /// vfs_data下，存放main package 中所有组的assetbundle的hash文件的目录名
        /// </summary>
        public const string MainPackage_AssetBundle_Hash_Files_Folder = "Hashs";

        public static readonly string AssetsHashFileName = "assets_hash.json";
        public static readonly string ExtensionGroupAssetsHashFolderName = "ExtensionGroupAssetsHash";

        /// <summary>
        /// vfs 主文件夹名
        /// </summary>
        public const string VFS_FOLDER_MAIN = "vfs_root";
        /// <summary>
        /// vfs 扩展包文件夹名
        /// </summary>
        public const string VFS_FOLDER_EXTENSION = "vfs_extensions";
        public const string VFS_FOLDER_DATA = "vfs_data";

        public const string VFS_STREAMINGASSETS_PATH = "TinaX_VFS"; //Assets/StreamingAssets/TinaX_VFS

        public const string AssetBundleManifestFileName = "VFSManifest.json";
        public const string MainPackage_AssetBundleManifests_Folder = "Manifests";

        /// <summary>
        /// 放在扩展组根目录下，表示扩展组信息的文件
        /// </summary>
        public static readonly string VFS_Data_ExtensionGroupInfo_FileName = "GroupInfo.json";

        public const string PakcageVersionFileName = "package_version.json";

        public const string BuildInfoFileName = "build_info.json";

    }
}


