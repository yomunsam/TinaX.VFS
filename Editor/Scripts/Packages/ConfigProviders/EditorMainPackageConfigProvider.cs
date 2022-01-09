using TinaX.VFS.ConfigAssets.Configurations;
using TinaX.VFS.Packages.ConfigProviders;
using TinaX.VFS.SerializableModels.Configurations;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Packages.ConfigProviders
{
    public class EditorMainPackageConfigProvider : MainPackageConfigProvider, IEditorConfigProvider<MainPackageConfig, MainPackageConfigModel>
    {
        protected readonly MainPackageConfig m_EditorConfig;

        public EditorMainPackageConfigProvider(MainPackageConfig config, MainPackageConfigModel configModel) : base(configModel)
        {
            m_EditorConfig = config;
        }

        public MainPackageConfig EditorConfiguration => m_EditorConfig;

    }
}
