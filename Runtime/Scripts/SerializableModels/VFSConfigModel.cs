using System;
using TinaX.VFS.SerializableModels.Configurations;

namespace TinaX.VFS.SerializableModels
{
    /// <summary>
    /// 可序列化模型 - VFS配置
    /// 包含VFS全局配置和VFS主包资产配置
    /// </summary>
    [Serializable]
    public class VFSConfigModel
    {
        /// <summary>
        /// 全局资产配置
        /// </summary>
        public GlobalAssetConfigModel GlobalAssetConfig = new GlobalAssetConfigModel();

        /// <summary>
        /// 主包配置
        /// </summary>
        public MainPackageConfigModel MainPackage = new MainPackageConfigModel();
    }
}
