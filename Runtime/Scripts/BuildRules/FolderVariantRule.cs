using System;
using System.Collections.Generic;

namespace TinaX.VFS.BuildRules
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2235 // Mark all non-serializable fields


    /// <summary>
    /// 资产文件夹 变体规则
    /// </summary>
    [Serializable]
    public struct FolderVariantRule
    {
        /// <summary>
        /// 根源 资产文件夹路径
        /// </summary>
        public string SourceFolderPath;
        public List<FolderVariant> Variants;
    }

#pragma warning restore CA2235 // Mark all non-serializable fields
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
