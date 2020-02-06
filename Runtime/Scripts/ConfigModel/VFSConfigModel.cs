using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TinaX.VFSKitInternal;

namespace TinaX.VFSKit
{
#if TINAX_DEBUG_DEV
    [CreateAssetMenu(fileName = "vfsConfig",menuName = "TinaX/Development/VFS/创建Config文件")]
#endif
    public class VFSConfigModel : ScriptableObject
    {
        public VFSGroupOption[] Groups = { VFSGroupOption.New() };
        public bool EnableWebVFS = false;

        public string[] GlobalVFS_Ignore_ExtName = ArrayUtil.Combine<string>(InternalVFSConfig.GlobalIgnoreExtName,new string[] 
        {
            "exe",
            "doc",
            "docx",
            "apk",
            "so"
        });

        /*
         * Path Item的定义: 
         * 如一个path: "Assets/Image/img1.png"
         * 
         * 其中“Assets” 、 “Image” 就是Path Item
         * 
         */
        public string[] GlobalVFS_Ignore_Path_Item =
        {
            "Resources",
            "Editor"
        };


        internal string[] GlobalVFS_Ignore_Path_Item_Lower
        {
            get
            {
                if (GlobalVFS_Ignore_Path_Item == null) 
                    return Array.Empty<string>();
                string[] arr = new string[GlobalVFS_Ignore_Path_Item.Length];
                for(var i = 0; i < GlobalVFS_Ignore_Path_Item.Length; i++)
                {
                    arr[i] = GlobalVFS_Ignore_Path_Item[i].ToLower();
                }
                return arr;
            }
        }

        private string[] _globalVFS_ignore_path_item_lower;

        public string[] GetGlobalVFS_Ignore_Path_Item(bool lower = false, bool forceRefresh = false)
        {
            if (!lower)
                return GlobalVFS_Ignore_Path_Item;
            else
            {
                if (forceRefresh)
                    return GlobalVFS_Ignore_Path_Item_Lower;
                else
                {
                    if(_globalVFS_ignore_path_item_lower != null && GlobalVFS_Ignore_Path_Item  != null && _globalVFS_ignore_path_item_lower.Length == GlobalVFS_Ignore_Path_Item.Length)
                        return _globalVFS_ignore_path_item_lower;
                    else
                    {
                        _globalVFS_ignore_path_item_lower = GlobalVFS_Ignore_Path_Item_Lower;
                        return _globalVFS_ignore_path_item_lower;
                    }
                }
            }
        }

    }

}
