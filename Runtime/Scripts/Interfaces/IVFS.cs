using System;
using System.Threading.Tasks;
using TinaX.VFSKit.Exceptions;
using TinaX.VFSKitInternal;
using UnityEngine;

namespace TinaX.VFSKit
{
    public interface IVFS
    {
        
        VFSCustomizable Customizable { get; }
        string DownloadWebAssetUrl { get; }
        XRuntimePlatform Platform { get; }
        string PlatformText { get; }
        int DownloadWebAssetTimeout { get; set; }




        #region About GC
        void Release(UnityEngine.Object asset);
        void UnloadUnusedAssets();

        #endregion


        #region Groups
        IGroup[] GetAllGroups();

        bool TryGetGroup(string groupName, out IGroup group);

        #endregion

        #region Extension Packages and Groups

        string[] GetExtensionPackagesInVirtualDisk();
        Task<bool> AddExtensionPackage(string group_name, bool available_web_vfs = true);
        Task<bool> AddExtensionPackageByPath(string extension_package_path, bool available_web_vfs = true);
        void AddExtensionPackage(string group_name, Action<bool,VFSException> callback);
        string[] GetActiveExtensionGroupNames();
        #endregion

        #region Load IAsset Sync
        IAsset LoadAsset<T>(string assetPath) where T : UnityEngine.Object;
        IAsset LoadAsset(string assetPath, Type type);
        #endregion

        #region Load Asset Sync
        T Load<T>(string assetPath) where T : UnityEngine.Object;
        UnityEngine.Object Load(string assetPath, Type type);
        #endregion

        #region Load IAsset Async Task
        Task<IAsset> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object;
        Task<IAsset> LoadAssetAsync(string assetPath, Type type);
        #endregion

        #region Load Asset Async Task
        Task<T> LoadAsync<T>(string assetPath) where T : UnityEngine.Object;
        Task<UnityEngine.Object> LoadAsync(string assetPath, Type type);
        #endregion

        #region Load IAsset Async Callback

        void LoadAssetAsync<T>(string assetPath, Action<IAsset> callback) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string assetPath, Action<IAsset, VFSException> callback) where T : UnityEngine.Object;
        void LoadAssetAsync(string assetPath, Type type, Action<IAsset, VFSException> callback);
        void LoadAssetAsync(string assetPath, Type type, Action<IAsset> callback);

        #endregion

        #region Load Asset Async Callback
        void LoadAsync<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object;
        void LoadAsync<T>(string assetPath, Action<T, VFSException> callback) where T : UnityEngine.Object;
        void LoadAsync(string assetPath, Type type, Action<UnityEngine.Object, VFSException> callback);
        void LoadAsync(string assetPath, Type type, Action<UnityEngine.Object> callback);



        #endregion

        #region Patch
        void InstallPatch(string path);

        #endregion
    }
}

