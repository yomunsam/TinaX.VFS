using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Utils;
using TinaXEditor.VFS.Managers.Config;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.ConfigProviders;
using TinaXEditor.VFS.Packages.Managers;
using TinaXEditor.VFS.Querier;
using TinaXEditor.VFS.Querier.Pipelines;

namespace TinaXEditor.VFS.Managers
{
    [UnityEditor.InitializeOnLoad]
    public static class EditorVFSManager
    {
        private static EditorAssetQuerier m_AssetQuerier;
        private static EditorMainPackage m_EditorMainPack;
        private static VFSConfigTpl m_VFSConfigTpl;

        static EditorVFSManager()
        {

        }

        public static EditorAssetQuerier AssetQuerier => m_AssetQuerier;

        public static VFSConfigTpl VFSConfig => m_VFSConfigTpl;

        public static EditorMainPackage MainPackage => m_EditorMainPack;

        public static EditorExpansionPackManager ExpansionPackManager => null;


        public static void InitializeAssetQuerier()
        {
            if (m_EditorMainPack == null)
                InitializeMainPack();
            if (m_AssetQuerier != null)
                return;
            m_AssetQuerier = new EditorAssetQuerier(EditorQueryAssetPipelineDefault.CreateDefault(), m_EditorMainPack, new EditorExpansionPackManager(), m_VFSConfigTpl.GlobalAssetConfig); //Todo:这儿看看以后搞成可扩展
            
        }

        public static void InitializeMainPack()
        {
            if (m_EditorMainPack != null)
                return;
            if (m_VFSConfigTpl == null)
                InitializeVFSConfigTpl();
            if (m_VFSConfigTpl == null)
                return;
            EditorMainPackageConfigProvider provider = new EditorMainPackageConfigProvider(m_VFSConfigTpl.MainPackage);
            provider.Standardize();
            m_EditorMainPack = new EditorMainPackage(provider);
            m_EditorMainPack.Initialize(); //初始化
        }

        public static void InitializeVFSConfigTpl()
        {
            if (m_VFSConfigTpl != null)
                return;
            var vfs_conf_asset = EditorVFSConfigManager.ConfigAsset;
            if (vfs_conf_asset == null)
                return;
            VFSConfigUtils.MapToVFSConfigTpl(in vfs_conf_asset, out m_VFSConfigTpl);
        }

        public static void RefreshConfiguration()
        {
            EditorVFSConfigManager.Clear();
            m_VFSConfigTpl = null;
            m_EditorMainPack = null;

            InitializeMainPack();
        }

        public static EditorAssetQueryResult QueryAsset(string assetPath)
        {
            return m_AssetQuerier.QueryAsset(assetPath);
        }

    }
}
