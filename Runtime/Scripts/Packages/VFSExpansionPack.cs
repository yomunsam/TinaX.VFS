using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;

namespace TinaX.VFS.Packages
{
    /// <summary>
    /// VFS 扩展包 类
    /// </summary>
    public class VFSExpansionPack : VFSPackage
    {
        protected ExpansionPackConfigTpl m_ExpansionPackConfig;
        public VFSExpansionPack(IConfigProvider<ExpansionPackConfigTpl> configProvider) : base(configProvider.Configuration)
        {
            m_ExpansionPackConfig = configProvider.Configuration;
        }

        public virtual string PackageName => m_ExpansionPackConfig.Name;

    }
}
