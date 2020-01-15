using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TinaX.VFSKit
{
#if TINAX_DEBUG_DEV
    [CreateAssetMenu(fileName = "vfsConfig",menuName = "TinaX/Development/VFS/创建Config文件")]
#endif
    public class VFSConfigModel : ScriptableObject
    {
        public VFSGroupOption[] Groups = { VFSGroupOption.New() };
        public bool EnableWebVFS = false;

        public string[] GlobalVFS_Ignore_ExtName =
        {
            "exe",
            "doc",
            "docx",
            "apk",
            "so"
        };
    }

}
