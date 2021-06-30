using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TinaX.VFSKit;
using TinaX;
using TinaX.VFSKit.Exceptions;
using Cysharp.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// VFS内部的一个资源
    /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class VFSAsset : IAsset, IRefCounter
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        protected UnityEngine.Object _asset;
        public UnityEngine.Object Asset => _asset;
        public string AssetPathLower => this.QueryResult.AssetPathLower;
        public string AssetPath => this.QueryResult.AssetPath;
        public VFSBundle Bundle { get; internal set; }
        public VFSGroup Group { get; internal set; }
        public AssetLoadState LoadState { get; internal set; } = AssetLoadState.Idle;
        public AssetQueryResult QueryResult { get; internal set; }

        //public UniTask _loadTask;
        //public UniTask LoadTask
        //{
        //    get
        //    {
        //        return _loadTask;
        //    }
        //    internal set
        //    {
        //        _loadTask = value.Preserve();
        //    }
        //}

        public AsyncLazy LoadTask { get; internal set; }

        public int AssetHashCode { get; private protected set; } = -1;

        public VFSAsset(VFSGroup group, AssetQueryResult queryResult)
        {
            this.Group = group;
            this.QueryResult = queryResult;
        }


        #region Counter
        public int RefCount { get; private set; } = 0;

        public void Release()
        {
            RefCount--;

            if(RefCount <= 0 && LoadState != AssetLoadState.Unloaded)
            {
                Unload();
            }
        }

        public void Retain()
        {
            RefCount++;
        }


        #endregion

        public T Get<T>() where T : UnityEngine.Object
        {
            return _asset as T;
        }

        public UnityEngine.Object Get() => _asset;

        public void Unload()
        {
            if(_asset != null)
            {
                if (!(_asset is GameObject))
                    Resources.UnloadAsset(_asset);
                _asset = null;
            }
            if (this.Bundle != null)
            {
                this.Bundle.Release();
                this.Bundle.UnRegisterAsset(this);
                this.Bundle = null;
            }

            this.LoadState = AssetLoadState.Unloaded;
        }
        


        public async UniTask LoadAsync<T>()
        {
            if (this.Bundle == null)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle is null, asset :" + AssetPathLower);
            if(this.Bundle.LoadState != AssetLoadState.Loaded)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle not ready, asset :" + AssetPathLower);
            if (this.LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded asset :" + AssetPathLower);
            if (this._asset == null || this.LoadState != AssetLoadState.Loaded)
            {
                this.LoadState = AssetLoadState.Loading;
                this._asset = await this.Bundle.AssetBundle.LoadAssetAsync<T>(this.AssetPathLower);
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask.ToAsyncLazy();
            this.AssetHashCode = this._asset.GetHashCode();
            RegisterToBundle();
        }

        public async UniTask LoadAsync(Type type)
        {
            if (this.Bundle == null)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle is null, asset :" + AssetPathLower);
            if (this.Bundle.LoadState != AssetLoadState.Loaded)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle not ready, asset :" + AssetPathLower);
            if (this.LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded asset :" + AssetPathLower);
            if (this._asset == null || this.LoadState != AssetLoadState.Loaded)
            {
                this.LoadState = AssetLoadState.Loading;
                this._asset = await this.Bundle.AssetBundle.LoadAssetAsync(this.AssetPathLower,type);
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask.ToAsyncLazy();
            this.AssetHashCode = this._asset.GetHashCode();
            RegisterToBundle();
        }

        public void Load<T>() where T : UnityEngine.Object
        {
            if (this.Bundle == null)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle is null, asset :" + AssetPathLower);
            if (this.Bundle.LoadState != AssetLoadState.Loaded)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle not ready, asset :" + AssetPathLower);
            if (this.LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded asset :" + AssetPathLower);
            if (this._asset == null || this.LoadState != AssetLoadState.Loaded)
            {
                this.LoadState = AssetLoadState.Loading;
                this._asset = this.Bundle.AssetBundle.LoadAsset<T>(this.AssetPathLower);
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask.ToAsyncLazy();
            this.AssetHashCode = this._asset.GetHashCode();
            RegisterToBundle();
        }

        public void Load(Type type)
        {
            if (this.Bundle == null)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle is null, asset :" + AssetPathLower);
            if (this.Bundle.LoadState != AssetLoadState.Loaded)
                throw new VFSException("[TinaX.VFS] Error: Load asset but assetbundle not ready, asset :" + AssetPathLower);
            if (this.LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded asset :" + AssetPathLower);
            if (this._asset == null || this.LoadState != AssetLoadState.Loaded)
            {
                this.LoadState = AssetLoadState.Loading;
                this._asset = this.Bundle.AssetBundle.LoadAsset(this.AssetPathLower, type);
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask.ToAsyncLazy();
            this.AssetHashCode = this._asset.GetHashCode();
            RegisterToBundle();
        }

        protected void RegisterToBundle()
        {
            if(this.Bundle != null)
            {
                this.Bundle.RegisterAsset(this);
            }
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            this.Release();
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// 用于从AssetDatabase加载的资源
    /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class EditorAsset : IAsset , IRefCounter
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        #region Editor not need counter
        public int RefCount { get; private set; } = 0;

        public void Release()
        {
            RefCount--;
            if(RefCount <= 0)
            {
                this.Unload();
            }
        }

        public void Retain()
        {
            RefCount++;
        }

        

        #endregion

        protected UnityEngine.Object _asset;
        public UnityEngine.Object Asset => _asset;

        public AssetLoadState LoadState { get; private set; } = AssetLoadState.Loaded;

        public string AssetPathLower { get; private set; }
        public int AssetHashCode { get; private set; }

        public EditorAsset(UnityEngine.Object asset,string lower_path)
        {
            _asset = asset;
            this.AssetPathLower = lower_path;
            this.AssetHashCode = asset.GetHashCode();
        }

        public EditorAsset(string path_lower)
        {
            this.AssetPathLower = path_lower;
        }

        public T Get<T>() where T : UnityEngine.Object
        {
            return _asset as T;
        }

        public UnityEngine.Object Get()
        {
            return _asset;
        }

        

        private void Unload()
        {
            if (_asset != null)
            {
                if (!(_asset is GameObject))
                {
                    Resources.UnloadAsset(_asset);
                }
                this._asset = null;
            }

            this.LoadState = AssetLoadState.Unloaded;
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            this.Release();
        }

        

    }
#endif

}
