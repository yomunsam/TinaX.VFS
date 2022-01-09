﻿using System;
using System.Collections.Generic;

namespace TinaX.VFS.SerializableModels.Configurations
{
    /// <summary>
    /// 可序列化模型 - 全局资产配置
    /// </summary>
    [Serializable]
    public class GlobalAssetConfigModel
    {
        /// <summary>
        /// 默认AssetBundle变体（Variant）
        /// </summary>
        public string DefaultAssetBundleVariant;

        /// <summary>
        /// 全局忽略的文件夹名
        /// </summary>
        public List<string> IgnoreExtensions = new List<string>();

        /// <summary>
        /// 全局忽略的文件夹名
        /// </summary>
        public List<string> IgnoreFolderNames = new List<string>();
    }
}
