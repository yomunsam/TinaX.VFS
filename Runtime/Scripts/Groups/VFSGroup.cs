using TinaX.VFS.BuildRules;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;

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

        public GroupConfigTpl Configuration => m_Config;

        public string GroupName => m_Config.Name;

        /// <summary>
        /// 查询是否是匹配的AssetPath（我们这个Group是否应该处理这个路径的加载）
        /// 这是一个相对轻量的查询，该查询不会查询打包规则, 但会查询忽略规则
        /// </summary>
        /// <param name="assetPathLower">经过小写处理后的assetPath</param>
        /// <returns>如果assetPath可用，则返回true</returns>
        public virtual bool IsMatchedAssetPathLower(string assetPathLower)
        {
            //首先，Asset列表
            if (m_Config.AssetPaths.Contains(assetPathLower))
                return true; //规则：AssetPaths在组内部的优先级最高，不受忽略规则影响

            //是否是 FolderPaths 某一项的子路径
            for(int i = m_Config.FolderPaths.Count - 1; i >=0; i--)
            {
                string folderPath = m_Config.FolderPaths[i];
                if (assetPathLower.StartsWith(folderPath))
                {
                    //找到了匹配Folder项，查询忽略规则
                    bool ignored = false;
                    for (int j = m_Config.IgnoreSubPath.Count - 1; j >= 0; j--)
                    {
                        string ignorePath = m_Config.IgnoreSubPath[j];
                        if (ignorePath.Length > folderPath.Length)
                        {
                            if(assetPathLower.StartsWith(ignorePath))
                            {
                                //被忽略了
                                ignored = true;
                                break;
                            }
                        }
                        else 
                            break; //因为有排序，所以一旦倒序遍历到ignorePath长度比folderPath小的时候，再往下就没意义了
                    }
                    return !ignored;
                }
            }

            return false;
        }

        /// <summary>
        /// 查询是否是匹配的AssetPath（我们这个Group是否应该处理这个路径的加载）
        /// 这是一个相对轻量的查询，该查询不会查询打包规则, 但会查询忽略规则
        /// （如果你顺便有了一个已经小写的路径，就尽量调用IsMatchedAssetPathLower）
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public virtual bool IsMatchedAssetPath(string assetPath) => IsMatchedAssetPathLower(assetPath.ToLower());

        public virtual void GetBuildRuleByAssetPathLower(string assetPathLower, out FolderBuildType folderBuildType, out string folderName)
        {
            for (int i = 0; i < m_Config.FolderSpecialBuildRules.Count; i++)
            {
                if (assetPathLower.StartsWith(m_Config.FolderSpecialBuildRules[i].Path))
                {
                    if (m_Config.FolderSpecialBuildRules[i].FolderBuildType == FolderBuildType.Inherit)
                        continue; //不处理，在获取BuildRule的方法里不处理它，意味着忽略这条规则

                    folderBuildType = m_Config.FolderSpecialBuildRules[i].FolderBuildType;
                    folderName = m_Config.FolderSpecialBuildRules[i].Path;
                    return;
                }
            }
            folderBuildType = FolderBuildType.Normal;
            folderName = string.Empty;
        }

        
    }
}
