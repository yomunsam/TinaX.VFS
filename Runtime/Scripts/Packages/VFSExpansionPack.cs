using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;

namespace TinaX.VFS.Packages
{
    /// <summary>
    /// VFS 扩展包 类
    /// </summary>
    public class VFSExpansionPack : VFSPackage
    {
        public VFSExpansionPack(IConfigProvider<ExpansionPackConfigTpl> configProvider) : base(configProvider.Configuration)
        {
        }
    }
}
