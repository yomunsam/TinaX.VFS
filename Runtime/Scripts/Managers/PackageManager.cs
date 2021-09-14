using System.Collections.Generic;
using TinaX.VFS.Options;
using TinaX.VFS.Packages;

namespace TinaX.VFS.Managers
{
    /// <summary>
    /// 管理Packages的管理器
    /// </summary>
    public class PackageManager
    {
        private VFSMainPackage m_MainPackage;

        private List<VFSExtensionPackage> m_ExtensionPackages;


        public PackageManager()
        {

        }

        public PackageManager(VFSPackageOption mainPackageOption) : this()
        {
            m_MainPackage = new VFSMainPackage(mainPackageOption);
        }

    }
}
