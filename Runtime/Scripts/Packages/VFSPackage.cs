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
using TinaX.VFS.Querier;

namespace TinaX.VFS.Packages
{
    /*
     * VFS Package是对VFS 6.x中Group概念的进一步梳理。
     */

    /// <summary>
    /// VFS Package
    /// </summary>
    public class VFSPackage
    {
        protected readonly PackageConfigTpl m_Config;

        public VFSPackage(PackageConfigTpl configTpl)
        {
            m_Config = configTpl;
        }
        
        public PackageConfigTpl Configuration => m_Config;

        public List<VFSGroup> Groups { get; private set; } = new List<VFSGroup>();

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
                if (group.IsMatchedAssetPathLower(result.AssetPathLower))
                {
                    //某一个组命中了资产,也就是板上钉钉这个资产归我们管了，那这时候我们顺便把我们知道的信息给补全一下
                    //Todo：考虑下这里要不要也搞成Pipeline
                    result.ManagedPackage = this;
                    result.ManagedGroup = group;
                    result.HideDirectoryStructure = group.Configuration.HideDirectoryStructure;

                    //然后是AssetBundle变体的处理
                    if(group.TryGetVariant(result.AssetPath, result.AssetPathLower, out string variant, out string sourceAssetPath))
                    {
                        result.IsVariant = true;
                        result.VariantName = variant;
                        result.VariantSourceAssetPath = sourceAssetPath;
                        result.VariantSourceAssetPathLower = sourceAssetPath.ToLower();
                    }

                    //特殊构建规则
                    group.GetBuildRuleByAssetPathLower(result.AssetPathLower, out var buildType, out string folderName); //这儿得到的folderName的标准化后的格式，小写，斜杠"/"结尾
                    result.BuildType = buildType;
                    switch (buildType)
                    {
                        case BuildRules.FolderBuildType.Normal:
                            result.OriginalAssetBundleName = result.AssetPathLower;
                            result.AssetBundleName = result.AssetPathLower;
                            result.OriginalFileNameInAssetBundle = result.AssetPathLower;
                            result.FileNameInAssetBundle = result.AssetPathLower;
                            break;
                        case BuildRules.FolderBuildType.Whole:
                            string final_folder_name = folderName.TrimEnd('/');
                            result.OriginalAssetBundleName = final_folder_name;
                            result.AssetBundleName = final_folder_name;
                            result.OriginalFileNameInAssetBundle = result.AssetPathLower;
                            result.FileNameInAssetBundle = result.AssetPathLower;
                            break;
                        case BuildRules.FolderBuildType.Subfolders:
                            string sub_path = result.AssetPathLower.Substring(folderName.Length, result.AssetPathLower.Length - folderName.Length);
                            //上面这一步骤是切割出资产路径相当于folderName的子路径，如"assets/images/logo/logo1.png", 规则的folderName为"assets/images/"时，切割得到结果为"logo/logo1.png";
                            int first_slash_index = sub_path.IndexOf('/');
                            if(first_slash_index == -1)
                            {
                                //没有子目录咯，回退到Normal规则
                                result.OriginalAssetBundleName = result.AssetPathLower;
                                result.AssetBundleName = result.AssetPathLower;

                                result.OriginalFileNameInAssetBundle = result.AssetPathLower;
                                result.FileNameInAssetBundle = result.AssetPathLower;
                            }
                            else
                            {
                                //取子目录并设为AssetBundle名字(都是小写化的)
                                result.OriginalAssetBundleName = folderName + sub_path.Substring(0, first_slash_index);
                                result.AssetBundleName = result.OriginalAssetBundleName;

                                result.OriginalFileNameInAssetBundle = result.AssetPathLower;
                                result.FileNameInAssetBundle = result.AssetPathLower;
                            }
                            break;

                    }

                    return true;
                }
            }


            return false;
        }

    }
}
