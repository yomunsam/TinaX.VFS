using TinaX.VFS.ConfigAssets.Configurations;
using TinaX.VFS.Packages;
using TinaX.VFS.SerializableModels.Configurations;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Packages
{
    public class EditorExpansionPack : VFSExpansionPack
    {
        private readonly IEditorConfigProvider<ExpansionPackConfig, ExpansionPackConfigModel> m_EditorConfigProvider;

        public EditorExpansionPack(IEditorConfigProvider<ExpansionPackConfig, ExpansionPackConfigModel> configProvider) : base(configProvider)
        {
            m_EditorConfigProvider = configProvider;
        }
    }
}
