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
        /// 全局资产配置
        /// </summary>
        public GlobalAssetConfigTpl GlobalAssetConfig = new GlobalAssetConfigTpl();

        /// <summary>
        /// 主包配置
        /// </summary>
        public MainPackageConfigTpl MainPackage = new MainPackageConfigTpl();
    }
}