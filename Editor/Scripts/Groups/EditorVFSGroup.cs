using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Groups;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Groups
{
    public class EditorVFSGroup : VFSGroup
    {
        private readonly IEditorConfigProvider<GroupConfigTpl> m_EditorConfigProvider;

        public EditorVFSGroup(IEditorConfigProvider<GroupConfigTpl> configProvider) : base(configProvider)
        {
            this.m_EditorConfigProvider = configProvider;
        }
    }
}
