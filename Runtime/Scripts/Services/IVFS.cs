using System;
using System.Threading.Tasks;
using UObject = UnityEngine.Object;

namespace TinaX.VFSKit
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
        Task<T> LoadAsync<T>(string assetPath) where T : UObject;
        Task<UObject> LoadAsync(string assetPath, Type type);

        Task<IAsset> LoadAssetAsync<T>(string assetPath) where T : UObject;
        Task<IAsset> LoadAssetAsync(string assetPath, Type type);
        #endregion

        #region 加载Scene
        Task<ISceneAsset> LoadSceneAsync(string scenePath);
        void LoadSceneAsync(string scenePath, Action<ISceneAsset, XException> callback);

        ISceneAsset LoadScene(string scenePath);
        #endregion

        void Release(UObject asset);
    }
}

