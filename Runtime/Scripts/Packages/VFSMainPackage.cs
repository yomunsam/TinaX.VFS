using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;

namespace TinaX.VFS.Packages
{
    /// <summary>
    /// VFS 主包 类
    /// </summary>
    public class VFSMainPackage : VFSPackage
    {
        public VFSMainPackage(IConfigProvider<MainPackageConfigTpl> configProvider) : base(configProvider.Configuration)
        {
        }
    }
}
