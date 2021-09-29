using System.Collections.Generic;

namespace TinaX.VFS.Packages.Managers
{
    /// <summary>
    /// 扩展包管理器
    /// 这里面主要是管理所有已加载的扩展包
    /// </summary>
    public class ExpansionPackManager
    {
        protected readonly List<VFSExpansionPack> m_ExpansionPacks = new List<VFSExpansionPack>();
    }
}
