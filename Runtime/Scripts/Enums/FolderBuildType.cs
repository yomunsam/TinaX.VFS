using System;

namespace TinaX.VFSKit
{
    [Serializable]
    public enum FolderBuildType
    {
        /// <summary>
        /// 正常的打包模式
        /// </summary>
        normal,
        /// <summary>
        /// 将给定的文件夹整体打包
        /// </summary>
        whole,
        /// <summary>
        /// 打包子目录（将给定目录中每个子目录作为一个整体来打包
        /// </summary>
        sub_dir,
        
    }
}
