using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using TinaX.Container;
using TinaX.Core.Helper.Platform;
using TinaX.Core.Utils;
using TinaX.Exceptions;
using TinaX.Options;
using TinaX.Systems.Pipeline;
using TinaX.VFS.Assets;
using TinaX.VFS.Internal;
using TinaX.VFS.LoadAssets;
using TinaX.VFS.Loader.DataLoader;
using TinaX.VFS.Options;
using TinaX.VFS.Packages;
using TinaX.VFS.Packages.ConfigProviders;
using TinaX.VFS.Packages.Managers;
using TinaX.VFS.Pipelines.LoadAsset;
using TinaX.VFS.Querier;
using TinaX.VFS.Querier.Pipelines;
using TinaX.VFS.SerializableModels;
using TinaX.VFS.Utils;
using UnityEngine;
using UObject = UnityEngine.Object;

#nullable enable
namespace TinaX.VFS.Services
{
    public class VFSService : IVFS, IVFSInternal
    {
        //------------固定字段--------------------------------------------------------------------------
        private readonly VFSOptions m_Option;
        private readonly IServiceContainer m_Services;
        private readonly bool m_Hans;

        private readonly VFSAssetManager m_AssetManager;

        //------------构造方法--------------------------------------------------------------------------
        public VFSService(IOptions<VFSOptions> option, IServiceContainer services)
        {
            this.m_Option = option.Value;
            m_VirtualSpaceInStramingAssets = VFSUtils.GetStreamingAssetsVirtualSpacePath();
            m_VirtualSpaceInLocalStorage = VFSUtils.GetLocalStorageVirtualSpacePath(Path.Combine(UnityEngine.Application.persistentDataPath, "TinaX"));
            m_PlatformName = PlatformHelper.GetName(PlatformHelper.GetXRuntimePlatform(Application.platform));
            this.m_Services = services;

            m_LoadAssetAsyncPipeline = LoadAssetPipelineDefault.CreateAsyncDefault();

            m_Hans = LocalizationUtil.IsHans();
            m_AssetManager = new VFSAssetManager();

        }

        //------------私有字段--------------------------------------------------------------------------
        private bool m_Initialized;

        /// <summary>
        /// 在编辑器下，使用UnityEditor.AssetDatabase 加载资产而非AssetBundle方式？
        /// </summary>
        private bool m_LoadAssetViaEditor = false;

        private string m_VirtualSpaceInStramingAssets;
        private string m_VirtualSpaceInLocalStorage;
        private string m_PlatformName;

        private VFSConfigModel? m_ConfigModel;
        private VFSMainPackage? m_MainPack;
        private ExpansionPackManager? m_ExpansionPackManager;
        private AssetQuerier? m_AssetQuerier;

        private XPipeline<ILoadAssetAsyncHandler> m_LoadAssetAsyncPipeline;
        //------------公开方法（服务接口对外）--------------------------------------------------------------------------

        public async UniTask<IAsset> LoadAssetAsync(string loadPath, Type type, string? variant = null)
        {
            var asset = await DoLoadAssetAsync(loadPath, type, variant);
            return asset;
        }


        public AssetQueryResult QueryAsset(string loadPath, string? variant = null)
        {
            if (!m_Initialized)
                throw new XException("The method must be called after the VFS module has finished starting.");
            return m_AssetQuerier!.QueryAsset(loadPath, variant ?? m_ConfigModel!.GlobalAssetConfig.DefaultAssetBundleVariant);
        }

        public void Release(UObject asset)
        {
            throw new NotImplementedException();
        }

        //------------其他公开方法--------------------------------------------------------------------------


        #region 生命周期

        public async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (m_Initialized)
                return;
            if (!m_Option.Enable)
                return;

            //资产加载方式
#if UNITY_EDITOR
            switch (VFSLoadModeInEditor.LoadMode)
            {
                case AssetLoadModeInEditor.LoadViaAssetDatabase:    //编辑器下，使用UnityEditor.AssetDatabase直接加载资产
                    m_LoadAssetViaEditor = true;
                    break;
                case AssetLoadModeInEditor.OverrideVirtualSpaceInStreamingAssetsPath:
                    m_VirtualSpaceInStramingAssets = VFSLoadModeInEditor.OverrideVirtualSpaceInStreamingAssetsPath;
                    m_LoadAssetViaEditor = false;
                    break;
            }
#endif

            var dataLoader = new DataLoader(m_PlatformName, m_VirtualSpaceInStramingAssets, m_VirtualSpaceInLocalStorage, m_LoadAssetViaEditor);
            //加载主包配置（以及全局配置）
            m_ConfigModel = await dataLoader.LoadVFSConfigAsync();

            //初始化主包（扩展包由开发者调用加入）
            InitializeMainPack();

            //Todo:扩展包
            m_ExpansionPackManager = new ExpansionPackManager();

            //资产查询器
            m_AssetQuerier = new AssetQuerier(QueryAssetPipelineDefault.CreateDefault(), m_Services, m_ConfigModel.GlobalAssetConfig, m_MainPack!, m_ExpansionPackManager);


            m_Initialized = true;
        }

        #endregion


        //------------私有方法--------------------------------------------------------------------------
        private void InitializeMainPack()
        {
            if (m_MainPack != null)
                return;
            var mainPackConfigProvider = new MainPackageConfigProvider(m_ConfigModel!.MainPackage);
            mainPackConfigProvider.Standardize(); //标准化
            m_MainPack = new VFSMainPackage(mainPackConfigProvider);
            m_MainPack.Initialize();
        }


        /// <summary>
        /// 资产加载的内部方法
        /// </summary>
        /// <param name="loadPath"></param>
        /// <param name="type"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        private async UniTask<VFSAsset> DoLoadAssetAsync(string loadPath, Type type, string? variant = null, CancellationToken cancellationToken = default)
        {
            //context
            var context = new LoadAssetContext(m_AssetQuerier!, m_AssetManager)
            {
                HansLog = m_Hans,
            };
            //payload
            var payload = new LoadAssetPayload(loadPath, variant ?? m_ConfigModel!.GlobalAssetConfig.DefaultAssetBundleVariant);

            await m_LoadAssetAsyncPipeline.StartAsync(async handler =>
            {
                await handler.LoadAssetAsync(context, payload);
                return !context.BreakPipeline && !cancellationToken.IsCancellationRequested;
            });

            if (payload.LoadedAsset == null)
                throw new NotFoundException($"Load asset failed, not found : {loadPath}", loadPath);

            return payload.LoadedAsset!;
        }

    }
}
