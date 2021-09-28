using TinaX.VFS.BuildRules;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Scripts.Structs;

namespace TinaX.VFS.Groups
{
    /// <summary>
    /// VFS 资产组
    /// 这里是实际执行资产操作的地方
    /// </summary>
    public class VFSGroup
    {
        protected readonly IConfigProvider<GroupConfigTpl> m_ConfigProvider;
        protected readonly GroupConfigTpl m_Config;

        public VFSGroup(IConfigProvider<GroupConfigTpl> configProvider)
        {
            this.m_ConfigProvider = configProvider;
            m_ConfigProvider.Standardize();
            m_Config = configProvider.Configuration; //Todo: 我觉得可能不应该放在这儿
        }

        /// <summary>
        /// 查询是否是匹配的AssetPath（这个AssetPath是不是归我们Group管）
        /// 这是一个相对轻量的查询，该查询不会查询打包规则
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public virtual bool IsMatchedAssetPath(string assetPath, out VFSAssetPath vfsAssetPath)
        {
            vfsAssetPath = new VFSAssetPath(assetPath);
            //首先，Asset列表
            if (m_Config.AssetPaths.Contains(vfsAssetPath.AssetPathLower))
                return true;

            //是否是 FolderPaths 某一项的子路径
            for(int i = 0; i < m_Config.FolderPaths.Count; i++)
            {
                if (vfsAssetPath.AssetPathLower.StartsWith(m_Config.FolderPaths[i]))
                    return true;
            }

            return false;
        }

        public virtual void GetBuildRule(ref VFSAssetPath vfsAssetPath, out FolderBuildType folderBuildType, out string folderName)
        {
            for (int i = 0; i < m_Config.FolderSpecialBuildRules.Count; i++)
            {
                if (vfsAssetPath.AssetPathLower.StartsWith(m_Config.FolderSpecialBuildRules[i].Path))
                {
                    if (m_Config.FolderSpecialBuildRules[i].FolderBuildType == FolderBuildType.Inherit)
                        continue; //不处理，在获取BuildRule的方法里不处理它，意味着忽略这条规则

                    folderBuildType = m_Config.FolderSpecialBuildRules[i].FolderBuildType;
                    folderName = m_Config.FolderSpecialBuildRules[i].Path;
                    break;
                }
            }
            folderBuildType = FolderBuildType.Normal;
            folderName = string.Empty;
        }

    }
}
