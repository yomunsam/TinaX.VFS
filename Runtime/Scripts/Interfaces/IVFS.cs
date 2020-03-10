using System.Threading.Tasks;
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
        Task<IAsset> LoadAssetAsync<T>(string assetPath) where T : Object;
        Task<T> LoadAsync<T>(string assetPath) where T : Object;
        void Release(Object asset);
        bool TryGetGroup(string groupName, out VFSGroup group);
        void UnloadUnusedAssets();
    }
}

