using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TinaX.Exceptions;
using TinaX.Options;
using TinaX.VFS.Const;
using TinaX.VFS.Internal;
using UObject = UnityEngine.Object;


namespace TinaX.VFS.Services
{
    public class VFSService : IVFS, IVFSInternal, TinaX.Services.IAssetService
    {
        private readonly IOptions<VFSOption> m_Option;

        public VFSService(IOptions<VFSOption> option)
        {
            this.m_Option = option;
        }

        /// <summary>
        /// 实现了IAssetService内置方法的实现者名字
        /// </summary>
        string TinaX.Services.Builtin.Base.IBuiltinServiceBase.ImplementerName => VFSConst.ServiceName;

        

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
        public Task<T> LoadAsync<T>(string assetPath, CancellationToken cancellationToken = default) where T : UObject
        {
            throw new NotImplementedException();
        }

        public Task<UObject> LoadAsync(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }

        public Task<IAsset> LoadAssetAsync<T>(string assetPath) where T : UObject
        {
            throw new NotImplementedException();
        }

        public Task<IAsset> LoadAssetAsync(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 实现接口 加载Scene
        public Task<ISceneAsset> LoadSceneAsync(string scenePath)
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

        #region 生命周期

        public async UniTask StartAsync()
        {
            await UniTask.CompletedTask;
        }



        #endregion



        public void Release(UnityEngine.Object asset)
        {
            throw new NotImplementedException();
        }
    }
}
