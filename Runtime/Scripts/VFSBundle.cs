using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// AssetBundle 内部类
    /// </summary>
    public class VFSBundle
    {
        public string[] Dependencies { get; set; }
        public AssetBundle AssetBundle { get; set; }

        public string AssetBundleName { get; set; } // Path: assets/xxx/xxx.xxx

        public string LoadedPath { get; set; } //file://xxx/xxx or https://xxx/xxx or xxx://xxx/xxx



        //private BundlesManager bundlesManager;

    }
}
