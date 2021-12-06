using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaX.VFS.Utils;
using TinaXEditor.VFS.Groups;
using TinaXEditor.VFS.Groups.ConfigProviders;
using TinaXEditor.VFS.Querier;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Packages
{
    public class EditorMainPackage : VFSMainPackage
    {
        private readonly IEditorConfigProvider<MainPackageConfigTpl> m_EditorConfigProvider;

        public EditorMainPackage(IEditorConfigProvider<MainPackageConfigTpl> configProvider) : base(configProvider)
        {
            m_EditorConfigProvider = configProvider;
        }

        public List<EditorVFSGroup> EditorGroups { get; private set; } = new List<EditorVFSGroup>();

        public override void InitializeGroups()
        {
            if (Groups.Count > 0)
                return;
            for (var i = 0; i < m_Config.Groups.Count; i++)
            {
                var conf_provider = new EditorGroupConfigProvider(m_Config.Groups[i]);
                conf_provider.Standardize();
                var group = new EditorVFSGroup(conf_provider);
                this.Groups.Add(group);
                EditorGroups.Add(group);
            }
        }

        /// <summary>
        /// 【编辑器用】查询资产（调用时请确保AssetQueryResult中的AssetPath、AssetPathLower、AssetExtension已经准备好）
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool TryQueryAsset(ref EditorAssetQueryResult result)
        {
            //暂时先遍历Group，以后看看能不能初始化的时候给它们做一些预处理
            var group_enumerator = EditorGroups.GetEnumerator();
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
                    if (group.TryGetVariant(result.AssetPath, result.AssetPathLower, out string variant, out string sourceAssetPath))
                    {
                        result.IsVariant = true;
                        result.VariantName = variant;
                        result.VirtualAssetPath = sourceAssetPath;
                        result.VirtualAssetPathLower = sourceAssetPath.ToLower();
                        result.VariantSourceAssetPath = sourceAssetPath;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(result.VirtualAssetPath))
                            result.VirtualAssetPath = result.AssetPath;
                        if (string.IsNullOrEmpty(result.VirtualAssetPathLower))
                            result.VirtualAssetPathLower = result.AssetPathLower;
                    }

                    //特殊构建规则
                    group.GetBuildRuleByAssetPathLower(result.VirtualAssetPathLower, out var buildType, out string folderName); //这儿得到的folderName的标准化后的格式，小写，斜杠"/"结尾
                    result.BuildType = buildType;
                    switch (buildType)
                    {
                        case TinaX.VFS.BuildRules.FolderBuildType.Normal:
                            result.OriginalAssetBundleName = result.VirtualAssetPathLower;
                            result.AssetBundleName = result.OriginalAssetBundleName;
                            result.OriginalFileNameInAssetBundle = result.VirtualAssetPathLower;
                            result.FileNameInAssetBundle = result.OriginalFileNameInAssetBundle;
                            break;
                        case TinaX.VFS.BuildRules.FolderBuildType.Whole:
                            string final_folder_name = folderName.TrimEnd('/');
                            result.OriginalAssetBundleName = final_folder_name;
                            result.AssetBundleName = final_folder_name;
                            result.OriginalFileNameInAssetBundle = result.VirtualAssetPathLower;
                            result.FileNameInAssetBundle = result.VirtualAssetPathLower;
                            break;
                        case TinaX.VFS.BuildRules.FolderBuildType.Subfolders:
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
        /// 从构建输出目录中，复制AssetBundle到Virtual Space目录
        /// </summary>
        /// <param name="buildOutputFolder"></param>
        /// <param name="virtualSpace"></param>
        /// <returns></returns>
        public virtual Task CopyAssetBundlesFromBuildOutputFolderToVirtualSpace(string buildOutputFolder, string virtualSpace, string platformName, EditorAssetQueryResult[] queryResults)
        {
            string abRootFolderInVSpace = GetAssetBundleRootFolder(virtualSpace, platformName);
            foreach(var item in queryResults)
            {
                var fileName = VFSUtils.GetAssetBundleFileName(item.AssetBundleName, item.VariantName);
                var filePath_outputFolder = Path.Combine(buildOutputFolder, fileName);

                if(File.Exists(filePath_outputFolder))
                {
                    var targetFilePath = Path.Combine(abRootFolderInVSpace, fileName);
                    var targetFileFolder = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileFolder))
                        Directory.CreateDirectory(targetFileFolder);

                    if (File.Exists(targetFilePath))
                        File.Delete(targetFilePath);
                    File.Copy(filePath_outputFolder, targetFilePath);
                }
            }

            //Todo: 这玩意以后可以改成多线程的

            return Task.CompletedTask;
        }

    }
}
