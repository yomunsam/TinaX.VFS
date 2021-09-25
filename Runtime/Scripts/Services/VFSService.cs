using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TinaX.Exceptions;
using TinaX.Options;
using TinaX.Services.ConfigAssets;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Const;
using TinaX.VFS.Internal;
using TinaX.VFS.Options;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace TinaX.VFS.Services
{
    public class VFSService : IVFS, IVFSInternal, TinaX.Services.IAssetService
    {
        private readonly IOptions<VFSOption> m_Option;
        private readonly IConfigAssetService m_ConfigAssetService;

        public VFSService(IOptions<VFSOption> option,
            IConfigAssetService configAssetService)
        {
            this.m_Option = option;
            this.m_ConfigAssetService = configAssetService;
        }

        /// <summary>
        /// 实现了IAssetService内置方法的实现者名字
        /// </summary>
        string TinaX.Services.Builtin.Base.IBuiltinServiceBase.ImplementerName => VFSConsts.ServiceName;


        #region 接口实现 同步加载资产系列
        public UObject Load(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }

        public T Load<T>(string assetPath) where T : UObject
        {
            throw new NotImplementedException();
        }

        public IAsset LoadAsset(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }

        public IAsset LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 接口实现 异步加载 callback系列
        public void LoadAsync(string assetPath, Type type, Action<UObject, XException> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadAsync(string assetPath, Type type, Action<UObject> callback)
        {
            throw new NotImplementedException();
        }
        public void LoadAsync<T>(string assetPath, Action<T, XException> callback) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void LoadAsync<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync(string assetPath, Type type, Action<IAsset, XException> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync(string assetPath, Type type, Action<IAsset> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync<T>(string assetPath, Action<IAsset, XException> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync<T>(string assetPath, Action<IAsset> callback)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 实现接口 异步加载 async/await系列
        public UniTask<T> LoadAsync<T>(string assetPath, CancellationToken cancellationToken = default) where T : UObject
        {
            throw new NotImplementedException();
        }

        public UniTask<UObject> LoadAsync(string assetPath, Type type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public UniTask<IAsset> LoadAssetAsync<T>(string assetPath, CancellationToken cancellationToken = default) where T : UObject
        {
            throw new NotImplementedException();
        }

        public UniTask<IAsset> LoadAssetAsync(string assetPath, Type type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 实现接口 加载Scene
        public UniTask<ISceneAsset> LoadSceneAsync(string scenePath)
        {
            throw new NotImplementedException();
        }

        public void LoadSceneAsync(string path, Action<ISceneAsset, XException> callback)
        {
            throw new NotImplementedException();
        }

        public ISceneAsset LoadScene(string scenePath)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void Release(UnityEngine.Object asset)
        {
            throw new NotImplementedException();
        }

        #region 生命周期

        public async UniTask StartAsync(CancellationToken cancellationToken)
        {
            var options = m_Option.Value;
            if (!options.Enable)
                return;

            var vfs_config_asset = await LoadVFSConfigAssetAsync(options.ConfigAssetLoadPath, cancellationToken);
            if(vfs_config_asset == null)
            {
                throw new XException($"Failed to load configuration assets \"{options.ConfigAssetLoadPath}\" ");
            }
            if (!vfs_config_asset.Enable)
            {
                Debug.LogFormat("VFS is not enabled according to the configuration");
                return;
            }

            await UniTask.CompletedTask;
        }



        #endregion



        /// <summary>
        /// 运行时加载VFS的配置文件
        /// </summary>
        /// <param name="loadPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private UniTask<VFSConfigAsset> LoadVFSConfigAssetAsync(string loadPath, CancellationToken cancellationToken)
        {
            /*
             * TinaX.Core中的IConfigAssetService依赖IAssetService来加载AssetBundle中的配置文件
             * 而VFS正是IAssetService的实现者，因此在VFS的启动过程中，不能用IConfigAssetService来加载资产，
             * VFS的配置资产需要自行解决加载问题
             */

            return UniTask.FromResult<VFSConfigAsset>(null);

        }

    }
}
