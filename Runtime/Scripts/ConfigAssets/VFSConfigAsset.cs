using System.Collections.Generic;
using TinaX.VFS.Const;
using UnityEngine;

namespace TinaX.VFS.ConfigAssets
{
    /// <summary>
    /// VFS配置 资产
    /// </summary>
#if TINAX_DEV
    [CreateAssetMenu(fileName = VFSConst.DefaultConfigAssetName, menuName = "TinaX Dev/Create VFS Config Asset", order = 11)]
#endif
    public class VFSConfigAsset : ScriptableObject
    {
        public bool Enable;

        public string DefaultAssetBundleVariant;

        /// <summary>
        /// 全局忽略后缀
        /// </summary>
        public List<string> GlobalIgnoreExtensions = new List<string>();


        public List<VFSGroupConfigAsset> Groups = new List<VFSGroupConfigAsset>();

        public VFSConfigAsset()
        {
#if UNITY_EDITOR
            #region Default
            //用于在编辑器上首次生成这个Asset文件时给它一些默认值, 所以这部分代码仅编辑器下可用，出包之后会剔除掉节省体积

            Enable = true;
            DefaultAssetBundleVariant = VFSConst.DefaultAssetBundleVariant;

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
            GlobalIgnoreExtensions.AddRange(VFSConst.GlobalIgnoreExtensions);


            Groups.Add(new VFSGroupConfigAsset
            {

            });

            #endregion
#endif

        }

    }
}
