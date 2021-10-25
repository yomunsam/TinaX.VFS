using System;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;

namespace TinaX.VFS.Packages.ConfigProviders
{
    /// <summary>
    /// VFS 资产包 配置提供者
    /// </summary>
    public class MainPackageConfigProvider : IConfigProvider<MainPackageConfigTpl>
    {
        protected readonly MainPackageConfigTpl m_Config;

        public MainPackageConfigProvider(MainPackageConfigTpl configTpl)
        {
            this.m_Config = configTpl;
        }
        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public MainPackageConfigTpl Configuration => m_Config;

        public void Standardize()
        {
            if (Standardized)
                return;
            Standardized = true;
        }

        public bool TryCheckError(out ConfigError? error)
        {
            throw new NotImplementedException();
        }
    }
}
