using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Groups.Utils;

namespace TinaX.VFS.Groups.ConfigProviders
{
    /// <summary>
    /// VFS 资产组 配置提供者 的默认接口实现
    /// </summary>
    public class GroupConfigProvider : IConfigProvider<GroupConfigTpl>
    {
        protected readonly GroupConfigTpl m_Config;

        public GroupConfigProvider(GroupConfigTpl configTpl)
        {
            this.m_Config = configTpl;
        }
        public bool Standardized { get; private set; } = false;

        public bool CheckCompleted { get; private set; } = false;

        public GroupConfigTpl Configuration => m_Config;

        public virtual void Standardize()
        {
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
