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
        public string SourceFolderPath; //Runtime标准化的时候这里结尾加上斜杠（不要小写）
        public List<FolderVariant> Variants; //标准化时排序，按照FolderPath的文本长度从小到大往下排序
    }

#pragma warning restore CA2235 // Mark all non-serializable fields
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
