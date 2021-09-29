using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Packages
{
    public class EditorMainPackage : VFSMainPackage
    {
        private readonly IEditorConfigProvider<MainPackageConfigTpl> m_EditorConfigProvider;

        public EditorMainPackage(IEditorConfigProvider<MainPackageConfigTpl> configProvider) : base(configProvider)
        {
            m_EditorConfigProvider = configProvider;

        }
    }
}
