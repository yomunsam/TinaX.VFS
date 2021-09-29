using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TinaX.Exceptions;
using UObject = UnityEngine.Object;

namespace TinaX.VFS
{
    public interface IVFS
    {
        #region 同步加载系列

        UObject Load(string assetPath, Type type);
        T Load<T>(string assetPath) where T : UObject;

        IAsset LoadAsset(string assetPath, Type type);
        IAsset LoadAsset<T>(string assetPath) where T : UObject;

        #endregion

        #region 异步加载 callback系列
        void LoadAsync(string assetPath, Type type, Action<UObject, XException> callback);
        void LoadAsync(string assetPath, Type type, Action<UObject> callback);
        void LoadAsync<T>(string assetPath, Action<T, XException> callback) where T : UObject;
        void LoadAsync<T>(string assetPath, Action<T> callback) where T : UObject;

        void LoadAssetAsync(string assetPath, Type type, Action<IAsset, XException> callback);
        void LoadAssetAsync(string assetPath, Type type, Action<IAsset> callback);
        void LoadAssetAsync<T>(string assetPath, Action<IAsset, XException> callback);
        void LoadAssetAsync<T>(string assetPath, Action<IAsset> callback);
        #endregion

        #region 异步加载 async/await系列
        UniTask<T> LoadAsync<T>(string assetPath, CancellationToken cancellationToken = default) where T : UObject;
        UniTask<UObject> LoadAsync(string assetPath, Type type, CancellationToken cancellationToken = default);

        UniTask<IAsset> LoadAssetAsync(string assetPath, Type type, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步加载资产
        /// </summary>
        /// <typeparam name="TAsset">资产类型</typeparam>
        /// <param name="assetPath">加载路径</param>
        /// <param name="variant">变体</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask<IAsset<TAsset>> LoadAssetAsync<TAsset>(string assetPath, string variant = null, CancellationToken cancellationToken = default) where TAsset : UObject;
        #endregion

        #region 加载Scene
        UniTask<ISceneAsset> LoadSceneAsync(string scenePath);
        void LoadSceneAsync(string scenePath, Action<ISceneAsset, XException> callback);

        ISceneAsset LoadScene(string scenePath);
        #endregion

        void Release(UObject asset);
        
    }
}

