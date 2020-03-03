using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Async;
using TinaX.VFSKit.Exceptions;

namespace TinaX.VFSKit.Loader
{ 
    public class AssetBundleLoader : IAssetBundleLoader
    {
        public async Task<AssetBundle> LoadFromFileAsync(string path, bool loadFromStreamingAssets, uint crc, ulong offset)
        {
            _ = loadFromStreamingAssets;
            var req = AssetBundle.LoadFromFileAsync(path, crc, offset);
            await req;
            return req.assetBundle;
        }

        public AssetBundle LoadFromFile(string path, bool loadFromStreamingAssets, uint crc, ulong offset)
        {
            _ = loadFromStreamingAssets;
            return AssetBundle.LoadFromFile(path, crc, offset);
        }

        

    }
}
