using System;
using System.Threading.Tasks;
using TinaX.VFSKit.Exceptions;
using TinaX.VFSKitInternal;
using UnityEngine;

namespace TinaX.VFSKit
{
    public interface IVFS
    {
        string ConfigPath { get; set; }
        AssetLoadType ConfigLoadType { get; }
        VFSCustomizable Customizable { get; }
        string DownloadWebAssetUrl { get; }
        XRuntimePlatform Platform { get; }
        string PlatformText { get; }
        int DownloadWebAssetTimeout { get; set; }

        VFSGroup[] GetAllGroups();
        Task<IAsset> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string assetPath, Action<IAsset> callback) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string assetPath, Action<IAsset, VFSException> callback) where T : UnityEngine.Object;
        Task<T> LoadAsync<T>(string assetPath) where T : UnityEngine.Object;
        void LoadAsync<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object;
        void LoadAsync<T>(string assetPath, Action<T, VFSException> callback) where T : UnityEngine.Object;
        void Release(UnityEngine.Object asset);
        bool TryGetGroup(string groupName, out VFSGroup group);
        void UnloadUnusedAssets();
    }
}

