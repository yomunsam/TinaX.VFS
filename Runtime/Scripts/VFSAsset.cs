using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TinaX.VFSKit;
using TinaX;
using TinaX.VFSKit.Exceptions;
using UniRx.Async;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// VFS内部的一个资源
    /// </summary>
    public class VFSAsset : IAsset, IRefCounter
    {
        protected UnityEngine.Object _asset;
        public UnityEngine.Object Asset => _asset;
        public string AssetPathLower => this.QueryResult.AssetPathLower;
        public string AssetPath => this.QueryResult.AssetPath;
        public VFSBundle Bundle { get; internal set; }
        public VFSGroup Group { get; internal set; }
        public AssetLoadState LoadState { get; internal set; } = AssetLoadState.Idle;
        public AssetQueryResult QueryResult { get; internal set; }
        public UniTask LoadTask { get; internal set; }

        public int AssetHashCode { get; private set; } = -1;

        public VFSAsset(VFSGroup group, AssetQueryResult queryResult)
        {
            this.Group = group;
            this.QueryResult = queryResult;
        }


        #region Counter
        public int RefCount { get; private set; }

        public void Release()
        {
            RefCount--;
            Bundle.Release();
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
                this._asset = await this.Bundle.AssetBundle.LoadAssetAsync<T>(this.AssetPathLower);
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask;
            this.AssetHashCode = this._asset.GetHashCode();
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
                this._asset = await this.Bundle.AssetBundle.LoadAssetAsync(this.AssetPathLower,type);
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask;
            this.AssetHashCode = this._asset.GetHashCode();
        }

    }


#if UNITY_EDITOR
    /// <summary>
    /// 用于从AssetDatabase加载的资源
    /// </summary>
    public class EditorAsset : IAsset , IRefCounter
    {
        #region Editor not need counter
        public int RefCount { get; private set; } = 0;

        public void Release() { }

        public void Retain() { }

        

        #endregion

        protected UnityEngine.Object _asset;
        public UnityEngine.Object Asset => _asset;

        public EditorAsset(UnityEngine.Object asset)
        {
            _asset = asset;
        }


        public T Get<T>() where T : UnityEngine.Object
        {
            return _asset as T;
        }

        public UnityEngine.Object Get()
        {
            return _asset;
        }
    }
#endif

}
