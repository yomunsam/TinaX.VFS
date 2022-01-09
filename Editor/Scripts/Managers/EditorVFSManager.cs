using TinaX.VFS.ConfigAssets;
using TinaX.VFS.SerializableModels;
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
        private static VFSConfigAsset m_VFSConfigAsset;
        private static VFSConfigModel m_VFSConfigModel;
        private static bool m_VFSConfigInitialized = false;

        private static EditorAssetQuerier m_AssetQuerier;
        private static EditorMainPackage m_EditorMainPack;


        static EditorVFSManager()
        {

        }

        public static EditorAssetQuerier AssetQuerier => m_AssetQuerier;

        public static VFSConfigModel VFSConfigModel => m_VFSConfigModel;

        public static EditorMainPackage MainPackage => m_EditorMainPack;

        public static EditorExpansionPackManager ExpansionPackManager => null;


        public static void InitializeAssetQuerier()
        {
            if (m_EditorMainPack == null)
                InitializeMainPack();
            if (m_AssetQuerier != null)
                return;
            m_AssetQuerier = new EditorAssetQuerier(EditorQueryAssetPipelineDefault.CreateDefault(), m_EditorMainPack, new EditorExpansionPackManager(), m_VFSConfigModel.GlobalAssetConfig); //Todo:这儿看看以后搞成可扩展

        }

        public static void InitializeMainPack()
        {
            if (m_EditorMainPack != null)
                return;
            InitializeVFSConfig(); 
            if (m_VFSConfigModel == null)
                return;
            EditorMainPackageConfigProvider provider = new EditorMainPackageConfigProvider(m_VFSConfigAsset.MainPackage, m_VFSConfigModel.MainPackage);
            provider.Standardize();
            m_EditorMainPack = new EditorMainPackage(provider);
            m_EditorMainPack.Initialize(); //初始化
        }

        public static void InitializeVFSConfig()
        {
            if (m_VFSConfigInitialized)
                return;

            EditorVFSConfigManager.Clear();
            m_VFSConfigAsset = EditorVFSConfigManager.ConfigAsset;
            if (m_VFSConfigAsset != null)
            {
                VFSConfigUtils.MapToVFSConfigModel(in m_VFSConfigAsset, out m_VFSConfigModel);
            }

            m_VFSConfigInitialized = true;
        }

        public static void RefreshConfiguration()
        {
            EditorVFSConfigManager.Clear();
            m_VFSConfigModel = null;
            m_VFSConfigAsset = null;
            m_VFSConfigInitialized = false;
            m_EditorMainPack = null;

            InitializeMainPack();
        }

        public static EditorAssetQueryResult QueryAsset(string assetPath)
        {
            return m_AssetQuerier.QueryAsset(assetPath);
        }

    }
}
