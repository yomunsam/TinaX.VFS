using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;

namespace TinaX.VFS.Packages.ConfigProviders
{
    /// <summary>
    /// VFS 资产包 配置提供者
    /// </summary>
    public class PackageConfigProvider : IConfigProvider<PackageConfigTpl>
    {
        protected readonly PackageConfigTpl m_Config;

        public PackageConfigProvider(PackageConfigTpl configTpl)
        {
            this.m_Config = configTpl;
        }
        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public PackageConfigTpl Configuration => m_Config;

        public void Standardize()
        {
            throw new NotImplementedException();
        }

        public bool TryCheckError(out ConfigError? error)
        {
            throw new NotImplementedException();
        }
    }
}
