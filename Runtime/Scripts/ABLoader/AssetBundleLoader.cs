using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TinaX.VFSKit.Exceptions;
using TinaX.VFSKit.Network;
using TinaX.IO;

namespace TinaX.VFSKit.Loader
{ 
    public class AssetBundleLoader : IAssetBundleLoader
    {
        public async UniTask<AssetBundle> LoadAssetBundleFromFileAsync(string path, string assetbundleName)
        {
            var req = AssetBundle.LoadFromFileAsync(path);
            await req;
            return req.assetBundle;
        }

        public async UniTask<AssetBundle> LoadAssetBundleFromAndroidStreamingAssetsAsync(string path, string assetbundleName,string virtualDiskPath)
        {
            var req = AssetBundle.LoadFromFileAsync(path);
            await req;
            return req.assetBundle;
        }
        
        public async UniTask DownloadFile(string url, string save_path, int timeout)
        {
            try
            {
                XFile.DeleteIfExists(save_path);
                var req = UnityWebRequest.Get(url);
                req.timeout = timeout;
                req.downloadHandler = new DownloadHandlerVDisk(save_path);
                await req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                {
                    if (req.responseCode == 404)
                        throw new FileNotFoundException("file not found from web :" + url, url);
                    else
                        throw new DownloadNetworkException($"[{req.responseCode}]Failed to download file from web: {url} --> {req.error}", url, req.error, req.responseCode);
                }
            }
            catch(UnityWebRequestException e)
            {
                var req = e.UnityWebRequest;
                if(e.IsNetworkError || e.IsHttpError)
                {
                    if (req.responseCode == 404)
                        throw new FileNotFoundException("file not found from web :" + url, url);
                    else
                        throw new DownloadNetworkException($"[{req.responseCode}]Failed to download file from web: {url} --> {req.error}", url, req.error, req.responseCode);
                }
                throw e;
            }
        }

        public async UniTask<AssetBundle> LoadAssetBundleFromWebAsync(string path,string assetBundleName,int timeout)
        {
            var req = UnityWebRequestAssetBundle.GetAssetBundle(path);
            req.timeout = timeout;
            await req.SendWebRequest();
            if(req.isNetworkError || req.isHttpError)
            {
                if (req.responseCode == 404)
                    throw new FileNotFoundException("assetbundle not found from web :" + path, path);
                else
                    throw new DownloadNetworkException($"[{req.responseCode}]Failed to download assetbundle from web: {path} --> {req.error}", path, req.error, req.responseCode);
            }
            return DownloadHandlerAssetBundle.GetContent(req);
        }

        public AssetBundle LoadAssetBundleFromAndroidStreamingAssets(string path, string assetbundleName, string virtualDiskPath)
        {
            return AssetBundle.LoadFromFile(path);
        }

        public AssetBundle LoadAssetBundleFromFile(string path, string assetbundleName)
        {
            return AssetBundle.LoadFromFile(path);
        }
    }
}
