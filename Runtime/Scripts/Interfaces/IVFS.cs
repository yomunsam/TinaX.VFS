using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    public interface IVFS
    {


        /// <summary>
        /// Load Asset 加载资源
        /// </summary>
        /// <typeparam name="T">Asset Type 资源类型</typeparam>
        /// <param name="assetPath">Asset Path 资源路径地址</param>
        /// <returns></returns>
        T LoadAsset<T>(string assetPath) where T : UnityEngine.Object;

        /// <summary>
        /// Load Asset 加载资源
        /// </summary>
        /// <param name="assetPath">Asset Path 资源路径地址</param>
        /// <param name="type">Asset Type 资源类型</param>
        /// <returns></returns>
        UnityEngine.Object LoadAsset(string assetPath, System.Type type);

        Task<T> LoadAssetAsync<T>(string assetPath);

        Task<UnityEngine.Object> LoadAssetAsync(string assetPath, System.Type type);

        /// <summary>
        /// Load Asset Async (callback) | 异步加载资源（通过回调）
        /// </summary>
        /// <typeparam name="T">Asset Type 资源类型</typeparam>
        /// <param name="assetPath">Asset Path 资源路径地址</param>
        /// <param name="callback">Loaded callback 加载结束回调</param>
        void LoadAssetAsync<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object;

        /// <summary>
        /// Load Asset Async (callback) | 异步加载资源（通过回调）
        /// </summary>
        /// <param name="assetPath">Asset Path 资源路径地址</param>
        /// <param name="type">Asset Type 资源类型</param>
        /// <param name="callback">Loaded callback 加载结束回调</param>
        void LoadAssetAsync(string assetPath, System.Type type, Action<UnityEngine.Object> callback);

    }
}
