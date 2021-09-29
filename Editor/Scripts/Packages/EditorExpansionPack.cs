using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Packages
{
    public class EditorExpansionPack : VFSExpansionPack
    {
        private readonly IEditorConfigProvider<ExpansionPackConfigTpl> m_EditorConfigProvider;

        public EditorExpansionPack(IEditorConfigProvider<ExpansionPackConfigTpl> configProvider) : base(configProvider)
        {
            m_EditorConfigProvider = configProvider;
        }
    }
}
