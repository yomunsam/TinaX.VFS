using System.IO;
using TinaX.VFS.Consts;

namespace TinaX.VFS.Utils
{
    public static class VFSUtils
    {
        /// <summary>
        /// 获取AssetBundle文件名
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetBundleVariant"></param>
        /// <returns></returns>
        public static string GetAssetBundleFileName(string assetBundleName, string assetBundleVariant)
        {
            /*
             * 2021.11 此时此刻编写该功能的时候，Unity的Scriptable Build Pipeline 没有变体功能，所以变体这玩意是VFS自己实现的，
             * 考虑到以后变体可能出现的变化（或者消失），所以我们封装了这么一个方法
             */
            return string.Format("{0}.{1}", assetBundleName, assetBundleVariant);
        }

        /// <summary>
        /// 获取项目工程中（Editor）Virtual Space的存在路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectVirtualSpacePath()
        {
#if UNITY_EDITOR
            return Path.Combine(Directory.GetCurrentDirectory(), VFSConsts.ProjectVFSArchiveFolder, VFSConsts.VirtualSpaceFolderNameInProject);

#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 获取MainPackage在VirualSpace中存放AssetBundle的根目录
        /// </summary>
        /// <param name="virtualSpacePath"></param>
        /// <param name="platformName"></param>
        /// <returns></returns>
        public static string GetMainPackageAssetBundleRootFolder(string virtualSpacePath, string platformName)
            => Path.Combine(virtualSpacePath, platformName, "vfs_main", "root");

        /// <summary>
        /// 获取扩展包在VirualSpace中存放AssetBundle的根目录
        /// </summary>
        /// <param name="virtualSpacePath"></param>
        /// <param name="platformName"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string GetExpansionPackageAssetBundleRootFolder(string virtualSpacePath, string platformName, string packageName)
            => Path.Combine(virtualSpacePath, platformName, "expansions", packageName, "root");

        public static string GetMainPackageDataFolder(string virtualSpacePath, string platformName)
            => Path.Combine(virtualSpacePath, platformName, "vfs_main", "data");

        public static string GetVFSConfigModelFilePath(string mainPackageDataFolder)
            => Path.Combine(mainPackageDataFolder, VFSConsts.VFSConfigJsonFileName);

        public static string GetVFSBundleManifestFilePath(string mainPackageDataFolder)
            => Path.Combine(mainPackageDataFolder, VFSConsts.VFSBundleManifestFileName);
    }
}
