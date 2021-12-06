/*
 * This file is part of the "TinaX Framework".
 * https://github.com/yomunsam/TinaX
 *
 * (c) Nekonya Studio <me@yomunchan.moe> <yomunsam@nekonya.io>
 * https://nekonya.io
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

using System.Collections.Generic;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Groups;
using TinaX.VFS.Groups.ConfigProviders;
using TinaX.VFS.Querier;

namespace TinaX.VFS.Packages
{
    /*
     * VFS Package是对VFS 6.x中Group概念的进一步梳理。
     */

    /// <summary>
    /// VFS Package
    /// </summary>
    public abstract class VFSPackage
    {
        protected readonly PackageConfigTpl m_Config;

        public VFSPackage(PackageConfigTpl configTpl)
        {
            m_Config = configTpl;
        }
        
        public PackageConfigTpl Configuration => m_Config;

        public List<VFSGroup> Groups { get; private set; } = new List<VFSGroup>();

        public virtual void InitializeGroups()
        {
            if (Groups.Count > 0)
                return;
            for(var i = 0; i < m_Config.Groups.Count; i++)
            {
                var conf_provider = new GroupConfigProvider(m_Config.Groups[i]);
                conf_provider.Standardize();
                var group = new VFSGroup(conf_provider);
                this.Groups.Add(group);
            }
        }

        /// <summary>
        /// 查询资产（调用时请确保AssetQueryResult中的AssetPath、AssetPathLower、AssetExtension是好的）
        /// </summary>
        /// <param name="result"></param>
        /// <returns>如果我们包可以管理这个资产，则返回true</returns>
        public virtual bool TryQueryAsset(ref AssetQueryResult result)
        {
            //暂时没啥好办法，遍历Group吧
            var group_enumerator = Groups.GetEnumerator();
            while (group_enumerator.MoveNext())
            {
                var group = group_enumerator.Current;
                if (group.IsMatchedAssetPathLower(result.VirtualAssetPathLower))
                {
                    //某一个组命中了资产,也就是板上钉钉这个资产归我们管了，那这时候我们顺便把我们知道的信息给补全一下
                    //Todo：考虑下这里要不要也搞成Pipeline
                    result.ManagedPackage = this;
                    result.ManagedGroup = group;
                    result.HideDirectoryStructure = group.Configuration.HideDirectoryStructure;

                    //对于Runtime来说，没有变体的推导

                    //特殊构建规则
                    group.GetBuildRuleByAssetPathLower(result.VirtualAssetPathLower, out var buildType, out string folderName); //这儿得到的folderName的标准化后的格式，小写，斜杠"/"结尾
                    result.BuildType = buildType;
                    switch (buildType)
                    {
                        case BuildRules.FolderBuildType.Normal:
                            result.OriginalAssetBundleName = result.VirtualAssetPathLower;
                            result.AssetBundleName = result.OriginalAssetBundleName;
                            result.OriginalFileNameInAssetBundle = result.VirtualAssetPathLower;
                            result.FileNameInAssetBundle = result.OriginalFileNameInAssetBundle;
                            break;
                        case BuildRules.FolderBuildType.Whole:
                            string final_folder_name = folderName.TrimEnd('/');
                            result.OriginalAssetBundleName = final_folder_name;
                            result.AssetBundleName = final_folder_name;
                            result.OriginalFileNameInAssetBundle = result.VirtualAssetPathLower;
                            result.FileNameInAssetBundle = result.VirtualAssetPathLower;
                            break;
                        case BuildRules.FolderBuildType.Subfolders:
                            string sub_path = result.VirtualAssetPathLower.Substring(folderName.Length, result.VirtualAssetPathLower.Length - folderName.Length);
                            //上面这一步骤是切割出资产路径相当于folderName的子路径，如"assets/images/logo/logo1.png", 规则的folderName为"assets/images/"时，切割得到结果为"logo/logo1.png";
                            int first_slash_index = sub_path.IndexOf('/');
                            if (first_slash_index == -1)
                            {
                                //没有子目录咯，回退到Normal规则
                                result.OriginalAssetBundleName = result.VirtualAssetPathLower;
                                result.AssetBundleName = result.VirtualAssetPathLower;

                                result.OriginalFileNameInAssetBundle = result.VirtualAssetPathLower;
                                result.FileNameInAssetBundle = result.VirtualAssetPathLower;
                            }
                            else
                            {
                                //取子目录并设为AssetBundle名字(都是小写化的)
                                result.OriginalAssetBundleName = folderName + sub_path.Substring(0, first_slash_index);
                                result.AssetBundleName = result.OriginalAssetBundleName;

                                result.OriginalFileNameInAssetBundle = result.VirtualAssetPathLower;
                                result.FileNameInAssetBundle = result.VirtualAssetPathLower;
                            }
                            break;

                    }

                    return true;
                }
            }


            return false;
        }

        /// <summary>
        /// 获取本包在Virtual Space中的AssetBundle存储根目录
        /// </summary>
        /// <param name="virtualSpacePath"></param>
        /// <param name="platformName"></param>
        /// <returns></returns>

        public abstract string GetAssetBundleRootFolder(string virtualSpacePath, string platformName);

    }
}
