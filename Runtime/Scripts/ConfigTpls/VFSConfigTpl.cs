using System;
using System.Collections.Generic;

namespace TinaX.VFS.ConfigTpls
{
    /// <summary>
    /// VFS 全局配置模板
    /// </summary>
    [Serializable]
    public class VFSConfigTpl
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable;

        /// <summary>
        /// 默认AssetBundle变体（Variant）
        /// </summary>
        public string DefaultAssetBundleVariant;

        /// <summary>
        /// 全局忽略的文件夹名
        /// </summary>
        public List<string> GlobalIgnoreExtensions = new List<string>();

        /// <summary>
        /// 全局忽略的文件夹名
        /// </summary>
        public List<string> GlobalIgnoreFolderName = new List<string>();

        /// <summary>
        /// 主包配置
        /// </summary>
        public MainPackageConfigTpl MainPackage = new MainPackageConfigTpl();
    }
}
