using TinaX.VFSKit;
using UnityEngine;
using System.Collections.Generic;
using TinaX.VFSKit.Loader;
using UniRx.Async;
using System;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// AssetBundle 内部类
    /// </summary>
    public class VFSBundle : IRefCounter
    {
        public List<VFSBundle> Dependencies { get; set; } = new List<VFSBundle>();
        public AssetBundle AssetBundle { get; private set; }
        public AssetLoadState LoadState { get; private set; } = AssetLoadState.Idle;
        public string AssetBundleName { get; set; } // Path: assets/xxx/xxx.xxx

        public string LoadedPath { get; set; } //file://xxx/xxx or https://xxx/xxx or xxx://xxx/xxx
        public string VirtualDiskPath { get; set; }

        public GroupHandleMode GroupHandleMode { get; set; }

        public int RefCount { get; private set; } = 0;

        public UniTask LoadTask { get; private set; }

        public void Release()
        {
            RefCount--;
            if (RefCount <= 0 && LoadState != AssetLoadState.Unloaded)
            {
                AssetBundle?.Unload(true);
                LoadState = AssetLoadState.Unloaded;
                foreach(var item in Dependencies)
                    item.Release();

                Dependencies.Clear();
            }
        }

        /// <summary>
        ///  +1s, 苟...
        /// </summary>
        public void Retain()
        {
            RefCount++;
        }

        public IAssetBundleLoader ABLoader { get; internal set; }

        internal int DownloadTimeout { get; set; } = 10;
        //private BundlesManager bundlesManager;

        internal UniTask Load()
        {
            if (LoadState == AssetLoadState.Loaded || LoadState == AssetLoadState.Unloaded) return UniTask.CompletedTask;
            LoadTask = DoLoad();
            return LoadTask;
        }

        private async UniTask DoLoad()
        {
            LoadState = AssetLoadState.Loading;
            //正儿八经的开始加载了
            if(LoadedPath.StartsWith("jar:file://", StringComparison.Ordinal))
            {
                this.AssetBundle = await this.ABLoader.LoadAssetBundleFromAndroidStreamingAssetsAsync(LoadedPath, AssetBundleName, this.VirtualDiskPath);
            }
            else if(LoadedPath.StartsWith("http://", StringComparison.Ordinal) ||
                LoadedPath.StartsWith("https://", StringComparison.Ordinal) ||
                //LoadedPath.StartsWith("file://", StringComparison.Ordinal)||
                LoadedPath.StartsWith("ftp://", StringComparison.Ordinal))
            {
                if(this.GroupHandleMode == GroupHandleMode.RemoteOnly)
                {
                    this.AssetBundle = await this.ABLoader.LoadAssetBundleFromWebAsync(LoadedPath, AssetBundleName, this.DownloadTimeout);
                }
                else
                {
                    await this.ABLoader.DownloadFile(LoadedPath, VirtualDiskPath, DownloadTimeout);
                    this.AssetBundle = await this.ABLoader.LoadAssetBundleFromFileAsync(this.VirtualDiskPath, this.AssetBundleName);
                }
            }
            else
            {
                this.AssetBundle = await this.ABLoader.LoadAssetBundleFromFileAsync(LoadedPath, AssetBundleName);
            }
        }

    }
}
