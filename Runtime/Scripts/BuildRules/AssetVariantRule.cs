using System;
using System.Collections.Generic;

namespace TinaX.VFS.BuildRules
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2235 // Mark all non-serializable fields
    /// <summary>
    /// 资产变体规则
    /// </summary>
    [Serializable]
    public struct AssetVariantRule
    {
        /// <summary>
        /// 根源资产路径
        /// </summary>
        public string SourceAssetPath;

        public List<AssetVariant> Variants;
    }
#pragma warning restore CA2235 // Mark all non-serializable fields
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
