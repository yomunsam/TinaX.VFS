using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinaX.VFS.ConfigAssets
{
    /// <summary>
    /// VFS 组 配置资产
    /// </summary>
    [Serializable]
    public class VFSGroupConfigAsset
    {
        public string Name;

        /// <summary>
        /// Obfuscate Directory Structure | 混淆目录结构
        /// </summary>
        public bool ObfuscateDirectoryStructure;

        [Header("Asset folders in this group | 当前资源组下包含的资源目录.")]
        public List<string> FolderPath;

        public VFSGroupConfigAsset()
        {
#if UNITY_EDITOR
            //编辑器下，给新建的配置对象一些默认值
            Name = "common";
            ObfuscateDirectoryStructure = false;
            FolderPath = new List<string>();
#endif
        }
    }
}
