using TinaX.VFSKit;
using UnityEngine;
using System.Collections.Generic;

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
        public AssetQueryResult QueryResult { get; set; }

        public int RefCount { get; private set; } = 0;


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



        //private BundlesManager bundlesManager;

    }
}
