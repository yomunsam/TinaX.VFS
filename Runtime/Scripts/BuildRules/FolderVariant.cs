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
        public string Variant;  //这儿Runtime和Editor的标准化都给它弄成小写。

        /// <summary>
        /// 变体 资产文件夹 路径
        /// </summary>
        public string FolderPath; //这儿Runtime的标准化给弄成小写,并且结尾加斜杠

    }

#pragma warning restore CA2235 // Mark all non-serializable fields
#pragma warning restore CA1815 // Override equals and operator equals on value types

}
