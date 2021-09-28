using System;
using System.Collections.Generic;
using TinaX.VFS.BuildRules;

namespace TinaX.VFS.ConfigTpls
{
#pragma warning disable CA2235 // Mark all non-serializable fields

    /// <summary>
    /// 资源组 配置模板
    /// </summary>
    [Serializable]
    public class GroupConfigTpl
    {
        /// <summary>
        /// 组名
        /// </summary>
        public string Name;

        /// <summary>
        /// Hide Directory Structure | 隐藏目录结构
        /// </summary>
        public bool HideDirectoryStructure = false;

        /// <summary>
        /// 这个组的资产，是否接受补丁
        /// </summary>
        public bool Patchable = true;

        /// <summary>
        /// 资源组中包含的资产目录
        /// </summary>
        public List<string> FolderPaths = new List<string>();

        /// <summary>
        /// 资源组中包含的具体
        /// </summary>
        public List<string> AssetPaths = new List<string>();

        public List<string> IgnoreSubPath = new List<string>();


        public List<FolderBuildRule> FolderSpecialBuildRules = new List<FolderBuildRule>();

        /// <summary>
        /// 资产变体规则
        /// </summary>
        public List<AssetVariantRule> AssetVariants = new List<AssetVariantRule>();

        /// <summary>
        /// 资产变体（文件夹）规则
        /// </summary>
        public List<FolderVariantRule> FolderVariants = new List<FolderVariantRule>();

    }

#pragma warning restore CA2235 // Mark all non-serializable fields
}
