using TinaX.VFS.ConfigProviders;
using TinaX.VFS.SerializableModels.Configurations;
using TinaX.VFS.Utils;

namespace TinaX.VFS.Packages
{
    /// <summary>
    /// VFS 主包 类
    /// </summary>
    public class VFSMainPackage : VFSPackage
    {
        public VFSMainPackage(IConfigProvider<MainPackageConfigModel> configProvider) : base(configProvider.Configuration)
        {
        }

        /// <summary>
        /// 获取Virtual Space中，本主包的AssetBundle存储根目录
        /// </summary>
        /// <param name="virtualSpacePath"></param>
        /// <param name="platformName"></param>
        /// <returns></returns>
        public override string GetAssetBundleRootFolder(string virtualSpacePath, string platformName)
            => VFSUtils.GetMainPackageAssetBundleRootFolder(virtualSpacePath, platformName);

    }
}
