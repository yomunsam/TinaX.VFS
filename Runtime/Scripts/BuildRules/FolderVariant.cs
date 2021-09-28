using System;

namespace TinaX.VFS.BuildRules
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2235 // Mark all non-serializable fields

    /// <summary>
    /// 资产文件夹变体
    /// </summary>
    [Serializable]
    public struct FolderVariant
    {
        /// <summary>
        /// 变体名称
        /// </summary>
        public string Variant;

        /// <summary>
        /// 变体 资产文件夹 路径
        /// </summary>
        public string FolderPath;
    }

#pragma warning restore CA2235 // Mark all non-serializable fields
#pragma warning restore CA1815 // Override equals and operator equals on value types

}
