using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine;

namespace TinaX.VFSKit.Loader
{
    public interface IAssetBundleLoader
    {
        UniTask DownloadFile(string url, string save_path, int timeout);
        UniTask<AssetBundle> LoadAssetBundleFromAndroidStreamingAssetsAsync(string path, string assetbundleName, string virtualDiskPath);
        UniTask<AssetBundle> LoadAssetBundleFromFileAsync(string path, string assetbundleName);
        UniTask<AssetBundle> LoadAssetBundleFromWebAsync(string path, string assetBundleName, int timeout);
    }
}
