using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;
using UniRx;
using Cysharp.Threading.Tasks;
using TinaX.VFSKit.Exceptions;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TinaX.VFSKitInternal
{
    public class SceneAsset : VFSAsset , ISceneAsset
    {
        public SceneAsset(VFSGroup group, AssetQueryResult queryResult) : base(group, queryResult) { }

        public new UnityEngine.Object Get()
        {
            throw new VFSException("Cannot \"Get\" asset from a SceneAsset.");
        }

        public UniTask LoadAsync()
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
                this._asset = null;
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask.ToAsyncLazy();
            this.AssetHashCode = this.GetHashCode();
            base.RegisterToBundle();

            return UniTask.CompletedTask;
        }

        public void Load()
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
                this._asset = null;
            }
            this.LoadState = AssetLoadState.Loaded;
            this.LoadTask = UniTask.CompletedTask.ToAsyncLazy();
            this.AssetHashCode = this.GetHashCode();
            base.RegisterToBundle();
        }

        public void OpenScene(LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (this.Bundle == null)
                throw new VFSException("[TinaX.VFS] Error: Open scene but assetbundle is null, asset :" + AssetPathLower);
            if (this.Bundle.LoadState != AssetLoadState.Loaded)
                throw new VFSException("[TinaX.VFS] Error: Open scene but assetbundle not ready, asset :" + AssetPathLower);
            if (this.LoadState == AssetLoadState.Unloaded)
                throw new VFSException("[TinaX.VFS] Error: Attempt to load an unloaded asset :" + AssetPathLower);

            SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(this.AssetPath), loadMode);
        }

    }

#if UNITY_EDITOR

    public class EditorSceneAsset : EditorAsset, ISceneAsset
    {
        public string AssetPath;
        public EditorSceneAsset(string fullPath, string lower_path) : base(lower_path)
        {
            this.AssetPath = fullPath;
        }

        public void OpenScene(LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            EditorSceneManager.LoadSceneInPlayMode(this.AssetPath, new LoadSceneParameters(loadMode));
        }


    }

#endif

}
