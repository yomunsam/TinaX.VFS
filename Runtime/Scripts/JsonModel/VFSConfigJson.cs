using System;
using TinaX.VFSKit;

namespace TinaX.VFSKitInternal
{
    [Serializable]
    public class VFSConfigJson : IVFSConfig
    {
        public VFSGroupOption[] Groups = { VFSGroupOption.New() };
        public bool EnableVFS = false;

        public string AssetBundleFileExtension = InternalVFSConfig.default_AssetBundle_ExtName;

        public string[] GlobalVFS_Ignore_ExtName = ArrayUtil.Combine<string>(InternalVFSConfig.GlobalIgnoreExtName, new string[]
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
        public string[] GlobalVFS_Ignore_Path_Item = ArrayUtil.Combine<string>(InternalVFSConfig.GlobalIgnorePathItem, new string[]
        {
            "Resources",
        });



        public string[] GlobalVFS_Ignore_Path_Item_Lower
        {
            get
            {
                if (GlobalVFS_Ignore_Path_Item == null)
                    return Array.Empty<string>();
                string[] arr = new string[GlobalVFS_Ignore_Path_Item.Length];
                for (var i = 0; i < GlobalVFS_Ignore_Path_Item.Length; i++)
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
                    if (_globalVFS_ignore_path_item_lower != null && GlobalVFS_Ignore_Path_Item != null && _globalVFS_ignore_path_item_lower.Length == GlobalVFS_Ignore_Path_Item.Length)
                        return _globalVFS_ignore_path_item_lower;
                    else
                    {
                        _globalVFS_ignore_path_item_lower = GlobalVFS_Ignore_Path_Item_Lower;
                        return _globalVFS_ignore_path_item_lower;
                    }
                }
            }
        }

        #region Web VFS
        public bool InitWebVFSOnStart = true;

        public string DefaultWebVFSBaseUrl = "http://127.0.0.1:8080/files";


        #endregion


        #region Interface
        public VFSGroupOption[] PGroups { get => this.Groups; set { this.Groups = value; } }
        public bool PEnableVFS { get { return this.EnableVFS; } set { this.EnableVFS = value; } }
        public string PAssetBundleFileExtension { get { return this.AssetBundleFileExtension; } set { this.AssetBundleFileExtension = value; } }
        public string[] PGlobalVFS_Ignore_ExtName { get { return GlobalVFS_Ignore_ExtName; } set { this.GlobalVFS_Ignore_ExtName = value; } }
        public string[] PGlobalVFS_Ignore_Path_Item { get { return GlobalVFS_Ignore_Path_Item; } set { this.GlobalVFS_Ignore_Path_Item = value; } }
        public bool PInitWebVFSOnStart { get { return this.InitWebVFSOnStart; } set { this.InitWebVFSOnStart = value; } }
        public string PDefaultWebVFSBaseUrl { get { return this.DefaultWebVFSBaseUrl; } set { this.DefaultWebVFSBaseUrl = value; } }

        #endregion
    }
}
