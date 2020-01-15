using UnityEngine;

namespace TinaX.VFSKit
{
    public enum GroupHandleMode
    {
        [Header("Local Only | 仅本地")]
        LocalOnly               = 1,
        [Header("Local and updatable | 本地且可更新")]
        LocalAndUpdatable       = 2,
        [Header("Local or remote. | 本地或服务端的")]
        LocalOrRemote           = 3,
        
        /// <summary>
        /// Not cache.
        /// </summary>
        [Header("Remote Only （No Cache.）| 仅服务端 无缓存")]
        RemoteOnly              = 4,

    }
}