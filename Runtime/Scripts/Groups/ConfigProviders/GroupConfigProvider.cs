using TinaX.VFS.ConfigProviders;
using TinaX.VFS.Groups.Utils;
using TinaX.VFS.SerializableModels.Configurations;

namespace TinaX.VFS.Groups.ConfigProviders
{
    /// <summary>
    /// VFS 资产组 配置提供者 的默认接口实现
    /// </summary>
    public class GroupConfigProvider : IConfigProvider<GroupConfigModel>
    {
        protected readonly GroupConfigModel m_Config;

        public GroupConfigProvider(GroupConfigModel configTpl)
        {
            this.m_Config = configTpl;
        }
        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public GroupConfigModel Configuration => m_Config;

        public virtual void Standardize()
        {
            if (Standardized)
                return;
            GroupStandardizationUtil.StandardizeGroup(m_Config);
            Standardized = true;
        }

        public virtual bool TryCheckError(out ConfigError? error)
        {
            error = null;
            CheckCompleted = true;
            return false;
        }
    }
}
