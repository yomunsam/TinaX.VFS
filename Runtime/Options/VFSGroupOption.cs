using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace TinaX.VFSKit
{
    [Serializable]
    public class VFSGroupOption
    {
        internal static VFSGroupOption New() => new VFSGroupOption();

        public string GroupName = "common";

        [Header("Asset folders in this group | 当前资源组下包含的资源目录.")]
        public string[] FolderPaths;

        [Header("Asset file in this group | 当前资源组下的资源文件.")]
        public string[] AssetPaths;

        public GroupHandleMode GroupAssetsHandleMode = GroupHandleMode.LocalAndUpdatable;


        

    }
}

