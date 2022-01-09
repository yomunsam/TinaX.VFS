using System;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.SerializableModels.Configurations;

namespace TinaX.VFS.Packages.ConfigProviders
{
    /// <summary>
    /// VFS 资产包 配置提供者
    /// </summary>
    public class MainPackageConfigProvider : IConfigProvider<MainPackageConfigModel>
    {
        protected readonly MainPackageConfigModel m_Config;

        public MainPackageConfigProvider(MainPackageConfigModel configTpl)
        {
            this.m_Config = configTpl;
        }
        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public MainPackageConfigModel Configuration => m_Config;

        public virtual void Standardize()
        {
            if (Standardized)
                return;
            Standardized = true;
        }

        public virtual bool TryCheckError(out ConfigError? error)
        {
            throw new NotImplementedException();
        }
    }
}
