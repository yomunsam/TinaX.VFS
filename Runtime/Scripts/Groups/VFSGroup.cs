using TinaX.VFS.ConfigTpls;

namespace TinaX.VFS.Groups
{
    /// <summary>
    /// VFS 资产组
    /// 这里是实际执行资产操作的地方
    /// </summary>
    public class VFSGroup
    {
        protected readonly GroupConfigTpl m_GroupConfig;

        public VFSGroup(GroupConfigTpl groupConfig)
        {
            this.m_GroupConfig = groupConfig;
        }

        public GroupConfigTpl Config => m_GroupConfig;
    }
}
