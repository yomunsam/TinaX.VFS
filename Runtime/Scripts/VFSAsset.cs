using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TinaX.VFSKit;
using TinaX;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// VFS内部的一个资源
    /// </summary>
    public class VFSAsset : IRefCounter
    {
        public UnityEngine.Object Asset { get; private set; }
        public string AssetPath { get; private set; }
        public VFSBundle Bundle { get; internal set; }
        public AssetLoadState LoadState { get; private set; } = AssetLoadState.Idle;

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

        public void Unload()
        {
            if(Asset != null)
            {
                if (!(Asset is GameObject))
                    Resources.UnloadAsset(Asset);
                Asset = null;
            }

            this.LoadState = AssetLoadState.Unloaded;
        }
    }
}
