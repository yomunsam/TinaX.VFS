using System.Collections.Generic;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Const;
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
        public bool Enable;

        public string DefaultAssetBundleVariant;

        /// <summary>
        /// 全局忽略后缀
        /// </summary>
        public List<string> GlobalIgnoreExtensions = new List<string>();

        /// <summary>
        /// 全局忽略的文件夹名
        /// </summary>
        public List<string> GlobalIgnoreFolderName = new List<string>();

        [Header("Main Package")]
        public MainPackageConfigTpl MainPackage = new MainPackageConfigTpl();

        /// <summary>
        /// 扩展包
        /// </summary>
        public List<ExpansionPackConfigTpl> ExpansionPacks = new List<ExpansionPackConfigTpl>();

        public VFSConfigAsset()
        {
#if UNITY_EDITOR
            #region Default
            //用于在编辑器上首次生成这个Asset文件时给它一些默认值, 所以这部分代码仅编辑器下可用，出包之后会剔除掉节省体积

            Enable = true;
            DefaultAssetBundleVariant = VFSConsts.DefaultAssetBundleVariant;

            GlobalIgnoreExtensions.AddRange(new string[]
            {
                ".exe",
                ".apk",
                ".proto",
                ".docx",
                ".xlsx",
                ".dll",
                ".so"
            });
            GlobalIgnoreExtensions.AddRange(VFSConsts.GlobalIgnoreExtensions);

            GlobalIgnoreFolderName.AddRange(VFSConsts.GlobalIgnoreFolderName);
            #endregion
#endif

#if UNITY_EDITOR && !TINAX_CONFIG_NO_RESOURCES
            GlobalIgnoreFolderName.AddRange(new string[] { "Resources" });
#endif

        }

    }
}
