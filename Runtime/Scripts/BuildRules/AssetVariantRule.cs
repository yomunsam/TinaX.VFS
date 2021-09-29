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
        public string SourceAssetPath; //Runtime的标准化时候不会把这个变成小写

        public List<AssetVariant> Variants; //标准化时排序，按照AssetPath的文本长度从小到大往下排序
    }
#pragma warning restore CA2235 // Mark all non-serializable fields
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
