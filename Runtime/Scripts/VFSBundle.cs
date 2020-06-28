using TinaX.VFSKit;
using UnityEngine;
using System.Collections.Generic;
using TinaX.VFSKit.Loader;
using Cysharp.Threading.Tasks;
using System;
using TinaX.VFSKit.Exceptions;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// AssetBundle 内部类
    /// </summary>
    public class VFSBundle : IBundle, IDisposable
    {
        /// <summary>
        /// 存放的是直接依赖，如果有需要请递归
        /// </summary>
        public List<VFSBundle> Dependencies { get; set; } = new List<VFSBundle>();
        /// <summary>
        /// 这里是直接依赖，如果有需要请递归
        /// </summary>
        public string[] DependenciesNames { get; set; } = new string[0];    
        public AssetBundle AssetBundle { get; private set; }
        public AssetLoadState LoadState { get; internal set; } = AssetLoadState.Idle;
        public string AssetBundleName { get; set; } // Path: assets/xxx/xxx.xxx

        public string LoadedPath { get; set; } //file://xxx/xxx or https://xxx/xxx or xxx://xxx/xxx
        public string VirtualDiskPath { get; set; }

        public GroupHandleMode GroupHandleMode { get; set; }

        public int RefCount { get; internal set; } = 0;

        public UniTask LoadTask { get; internal set; }

        public List<VFSAsset> Assets = new List<VFSAsset>();

        public void Release()
        {
            RefCount--;
            foreach (var dep in this.Dependencies)
                dep.Release();

            if (RefCount <= 0 && LoadState != AssetLoadState.Unloaded)
            {
                this.Unload();
            }
        }

        public void Unload()
        {
            if(this.AssetBundle != null)
            {
                this.AssetBundle.Unload(true);
                this.AssetBundle = null;
            }
            LoadState = AssetLoadState.Unloaded;
            //foreach (var item in Dependencies)
            //    item.Release();
            Dependencies.Clear();
            DependenciesNames = null;
            this.ABLoader = null;
            this.LoadTask = UniTask.CompletedTask;
        }


        /// <summary>
        ///  +1s, 苟...
        /// </summary>
        public void Retain()
        {
            RefCount++;
            foreach (var dep in this.Dependencies)
                dep.Retain();
        }

        public IAssetBundleLoader ABLoader { get; internal set; }

        internal int DownloadTimeout { get; set; } = 10;
        //private BundlesManager bundlesManager;

        /// <summary>
        /// 同步加载
        /// </summary>
        internal void Load()
        {
            if (this.LoadState == AssetLoadState.Loaded && this.AssetBundle != null) return;
            if (LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded assetbundle :" + this.AssetBundleName);


            LoadState = AssetLoadState.Loading; //其实这时候改没啥用
            if (LoadedPath.StartsWith("jar:file://", StringComparison.Ordinal))
            {
                this.AssetBundle = this.ABLoader.LoadAssetBundleFromAndroidStreamingAssets(this.LoadedPath, this.AssetBundleName, this.VirtualDiskPath);
            }
            else if (LoadedPath.StartsWith("http://", StringComparison.Ordinal) ||
                LoadedPath.StartsWith("https://", StringComparison.Ordinal) ||
                //LoadedPath.StartsWith("file://", StringComparison.Ordinal)||
                LoadedPath.StartsWith("ftp://", StringComparison.Ordinal))
            {
                throw new VFSException("[TinaX.VFS] Error: Connot load from network sync");
            }
            else
            {
                this.AssetBundle = ABLoader.LoadAssetBundleFromFile(this.LoadedPath, this.AssetBundleName);
            }

            LoadState = AssetLoadState.Loaded;
        }
        internal async UniTask LoadAsync()
        {
            if (LoadState == AssetLoadState.Loaded && this.AssetBundle != null) return; //已加载
            if(LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded assetbundle :" + this.AssetBundleName);
            //开始加载
            await DoLoadAsync();

            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask;
        }

        private async UniTask DoLoadAsync()
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

        public void Dispose()
        {
            if(this.AssetBundle != null)
            {
                this.AssetBundle.Unload(true);
                this.AssetBundle = null;
            }
            this.ABLoader = null;
            this.LoadTask = UniTask.CompletedTask;
            this.Dependencies = null;
            this.DependenciesNames = null;
        }

        /// <summary>
        /// 当某个asset有此bundle成功load之后，将其注册进来
        /// </summary>
        /// <param name="asset"></param>
        internal protected void RegisterAsset(VFSAsset asset)
        {
            if (!this.Assets.Contains(asset))
            {
                this.Assets.Add(asset);
            }
        }

        internal protected void UnRegisterAsset(VFSAsset asset)
        {
            if (this.Assets.Contains(asset))
            {
                this.Assets.Remove(asset);
            }
        }


    }
}
