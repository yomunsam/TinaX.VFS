using TinaX.VFS.SerializableModels.Configurations;

namespace TinaX.VFS.ConfigProviders
{
    /// <summary>
    /// 全局资产 配置提供者 默认接口实现
    /// </summary>
    public class GlobalAssetConfigProvider : IConfigProvider<GlobalAssetConfigModel>
    {
        private readonly GlobalAssetConfigModel m_Config;

        public GlobalAssetConfigProvider(GlobalAssetConfigModel configTpl)
        {
            this.m_Config = configTpl;
        }

        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public GlobalAssetConfigModel Configuration => m_Config;

        public void Standardize()
        {
            Standardized = true;
        }

        public bool TryCheckError(out ConfigError? error)
        {
            error = null;
            CheckCompleted = true;
            return false;
        }
    }
}
