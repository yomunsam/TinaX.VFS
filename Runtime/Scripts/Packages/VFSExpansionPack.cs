using TinaX.VFS.ConfigProviders;
using TinaX.VFS.SerializableModels.Configurations;
using TinaX.VFS.Utils;

namespace TinaX.VFS.Packages
{
    /// <summary>
    /// VFS 扩展包 类
    /// </summary>
    public class VFSExpansionPack : VFSPackage
    {
        protected ExpansionPackConfigModel m_ExpansionPackConfig;
        public VFSExpansionPack(IConfigProvider<ExpansionPackConfigModel> configProvider) : base(configProvider.Configuration)
        {
            m_ExpansionPackConfig = configProvider.Configuration;
        }

        public virtual string PackageName => m_ExpansionPackConfig.Name;

        /// <summary>
        /// 获取本扩展包在Virtual Space中的AssetBundle存储根目录
        /// </summary>
        /// <param name="virtualSpacePath"></param>
        /// <param name="platformName"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string GetAssetBundleRootFolder(string virtualSpacePath, string platformName)
            => VFSUtils.GetExpansionPackageAssetBundleRootFolder(virtualSpacePath, platformName, m_ExpansionPackConfig.Name);
    }
}
