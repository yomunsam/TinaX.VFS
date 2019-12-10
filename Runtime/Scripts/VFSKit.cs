using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TinaX.VFSKit
{
    public class VFSKit : IVFS
    {
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public UnityEngine.Object LoadAsset(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public Task<UnityEngine.Object> LoadAssetAsync(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync(string assetPath, Type type, Action<UnityEngine.Object> callback)
        {
            throw new NotImplementedException();
        }
    }
}