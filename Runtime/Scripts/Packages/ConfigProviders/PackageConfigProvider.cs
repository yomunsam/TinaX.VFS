using System;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.SerializableModels.Configurations;

namespace TinaX.VFS.Packages.ConfigProviders
{
    /// <summary>
    /// VFS 资产包 配置提供者
    /// </summary>
    public class PackageConfigProvider : IConfigProvider<PackageConfigModel>
    {
        protected readonly PackageConfigModel m_Config;

        public PackageConfigProvider(PackageConfigModel configTpl)
        {
            this.m_Config = configTpl;
        }
        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public PackageConfigModel Configuration => m_Config;

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
