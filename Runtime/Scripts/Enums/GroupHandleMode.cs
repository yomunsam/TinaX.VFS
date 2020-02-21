using UnityEngine;

namespace TinaX.VFSKit
{
    public enum GroupHandleMode
    {
        [Header("Local Only | 仅本地")]            //资源必须保证在本地，且在当前母包中始终不会变动
        LocalOnly               = 1,
        [Header("Local and updatable | 本地且可更新")]    //资源必须保证在本地，并且可以后续更新（等更新完成后才会继续运行）
        LocalAndUpdatable       = 2,
        [Header("Local or remote. | 本地或服务端的")]  //资源可以存放在本地，也可以一开始就放在服务端，要用的时候再下载。
        LocalOrRemote           = 3,
        
        /// <summary>
        /// Not cache.
        /// </summary>
        [Header("Remote Only （No Cache.）| 仅服务端 无缓存")]
        RemoteOnly              = 4,

    }
}