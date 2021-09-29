using System;
using System.Collections.Generic;

namespace TinaX.VFS.ConfigTpls
{
    /// <summary>
    /// 全局资产配置模板
    /// </summary>
    [Serializable]
    public class GlobalAssetConfigTpl
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
        public List<string> IgnoreFolderName = new List<string>();
    }
}
