using System.IO;
using TinaX.VFSKit.Const;
using TinaX.VFSKitInternal;
using TinaX.VFSKitInternal.Utils;

namespace TinaX.VFSKit
{
    public class VFSExtensionGroup : VFSGroup
    {
        /// <summary>
        /// 是否有指定Package的路径
        /// </summary>
        public bool OverridePackagePath { get; private set; } = false;

        public bool WebVFS_Available { get; private set; } = true;

        public PackageVersionInfo VersionInfo { get; internal set; }
        public VFSUpgrdableVersionInfo UpgradableVersionInfo { get; internal set; }

        /// <summary>
        /// 如果有指定扩展包的路径，那么从这里获取到指定的路径
        /// </summary>
        public string PackagePathSpecified { get; private set; }
        public VFSExtensionGroup(VFSGroupOption option) : base(option) { }
        public VFSExtensionGroup(VFSGroupOption option, bool available_web_vfs) : base(option) { this.WebVFS_Available = available_web_vfs; }

        /// <summary>
        /// 手动指定Package路径
        /// </summary>
        /// <param name="option"></param>
        /// <param name="ExtensionPackageFolder"></param>
        public VFSExtensionGroup(VFSGroupOption option, string ExtensionPackageFolder, bool available_web_vfs) : base(option)
        {
            this.OverridePackagePath = true;
            this.PackagePathSpecified = ExtensionPackageFolder;
            this.WebVFS_Available = available_web_vfs;
        }

        public new string GetAssetBundlePath(string packages_root_path, string assetbundleName)
        {
            if (this.OverridePackagePath)
                return Path.Combine(this.PackagePathSpecified, assetbundleName); //这个Group使用指定的路径，它可能不在Packages的约定目录中
            else
                return base.GetAssetBundlePath(packages_root_path, assetbundleName);
        }


        /// <summary>
        /// 获取Manifest的文件地址【扩展组重写方法】
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <returns></returns>
        public new string GetManifestFilePath(string packages_root_path)
        {
            if (this.OverridePackagePath)
                return Path.Combine(this.PackagePathSpecified, VFSConst.AssetBundleManifestFileName);
            else
                return base.GetManifestFilePath(packages_root_path);
        }

        public string GetGroupRootFolder(string packages_root_path)
        {
            if (this.OverridePackagePath)
                return this.PackagePathSpecified;
            else
                return VFSUtil.GetExtensionGroupFolder(packages_root_path, this.GroupName);
        }

        public new string GetBuildInfoPath(string packages_root_path)
        {
            if (this.OverridePackagePath)
                return VFSUtil.GetExtensionGroup_BuildInfo_Path_InGroupPath(this.PackagePathSpecified);
            else
                return base.GetManifestFilePath(packages_root_path);
        }

        public string GetVersionInfoPath(string packages_root_path)
        {
            if (this.OverridePackagePath)
                return Path.Combine(this.PackagePathSpecified, VFSConst.PakcageVersionFileName);
            else
                return VFSUtil.GetExtensionGroup_VersionInfo_Path(packages_root_path, this.GroupName);
        }

        public string GetUpgradableVersionPath(string package_root_path)
        {
            if (this.OverridePackagePath)
                return Path.Combine(this.PackagePathSpecified, VFSConst.Upgradable_Vesion_FileName);
            else
                return Path.Combine(VFSUtil.GetExtensionGroupFolder(package_root_path, this.GroupName), VFSConst.Upgradable_Vesion_FileName);
        }

    }
}
