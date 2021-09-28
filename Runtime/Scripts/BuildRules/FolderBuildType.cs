using System;

namespace TinaX.VFS.BuildRules
{
    [Serializable]
    public enum FolderBuildType : short
    {
        /// <summary>
        /// 普通模式：一个Asset对应一个AssetBundle
        /// </summary>
        Normal      = 0,

        /// <summary>
        /// 整体：给定的文件夹整体打成一个AssetBundle
        /// </summary>
        Whole       = 1,

        /// <summary>
        /// 子文件夹：给定目录中每个子文件夹打成一个AssetBundle
        /// </summary>
        Subfolders  = 2,

        /// <summary>
        /// 继承：继承上级目录的规则
        /// </summary>
        Inherit = 3,
    }
}
