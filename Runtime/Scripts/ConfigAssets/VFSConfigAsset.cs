using System.Collections.Generic;
using TinaX.VFS.ConfigAssets.Configurations;
using TinaX.VFS.Consts;
using UnityEngine;

namespace TinaX.VFS.ConfigAssets
{
    /// <summary>
    /// VFS配置 资产
    /// </summary>
#if TINAX_DEV
    [CreateAssetMenu(fileName = VFSConsts.DefaultConfigAssetName, menuName = "TinaX Dev/Create VFS Config Asset", order = 11)]
#endif
    public class VFSConfigAsset : ScriptableObject
    {
        /// <summary>
        /// 全局资产配置
        /// </summary>
        public GlobalAssetConfig GlobalAssetConfig = new GlobalAssetConfig();

        [Header("Main Package")]
        public MainPackageConfig MainPackage = new MainPackageConfig();

        /// <summary>
        /// 扩展包
        /// </summary>
        public List<ExpansionPackConfig> ExpansionPacks = new List<ExpansionPackConfig>();

        public VFSConfigAsset()
        {
#if UNITY_EDITOR
            #region Default
            //用于在编辑器上首次生成这个Asset文件时给它一些默认值, 所以这部分代码仅编辑器下可用，出包之后会剔除掉节省体积

            //Enable = true;
            GlobalAssetConfig.DefaultAssetBundleVariant = VFSConsts.DefaultAssetBundleVariant;

            GlobalAssetConfig.IgnoreExtensions.AddRange(new string[]
            {
                ".exe",
                ".apk",
                ".proto",
                ".docx",
                ".xlsx",
                ".dll",
                ".so"
            });
            GlobalAssetConfig.IgnoreExtensions.AddRange(VFSConsts.GlobalIgnoreExtensions);

            GlobalAssetConfig.IgnoreFolderNames.AddRange(VFSConsts.GlobalIgnoreFolderName);
            #endregion
#endif

#if UNITY_EDITOR && !TINAX_CONFIG_NO_RESOURCES
            GlobalAssetConfig.IgnoreFolderName.AddRange(new string[] { "Resources" });
#endif

        }

    }
}
