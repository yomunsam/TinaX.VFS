using TinaX.VFS.ConfigAssets.Configurations;
using TinaX.VFS.Groups;
using TinaX.VFS.SerializableModels.Configurations;
using TinaXEditor.VFS.Scripts.ConfigProviders;

namespace TinaXEditor.VFS.Groups
{
    public class EditorVFSGroup : VFSGroup
    {
        private readonly IEditorConfigProvider<GroupConfig, GroupConfigModel> m_EditorConfigProvider;

        public EditorVFSGroup(IEditorConfigProvider<GroupConfig, GroupConfigModel> configProvider) : base(configProvider)
        {
            this.m_EditorConfigProvider = configProvider;
        }

        /// <summary>
        /// 获取所有被我们组管理的Asset们
        /// </summary>
        /// <param name="groupAssets"></param>
        //public virtual void GetAllManagedAssets(out List<AssetPathAndGuid> groupAssets)
        //{
        //    groupAssets = new List<AssetPathAndGuid>();

        //    var editorConfig = m_EditorConfigProvider.EditorConfiguration;

        //    //AssetPaths
        //    foreach(var assetPath in editorConfig.AssetPaths)
        //    {
        //        if (string.IsNullOrEmpty(assetPath))
        //            continue;
        //        var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
        //        if(!guid.Empty())
        //        {
        //            groupAssets.Add(new AssetPathAndGuid(assetPath, guid));
        //        }
        //    }

        //    //FolderPaths整理
        //    var guids = AssetDatabase.FindAssets("", editorConfig.FolderPaths.ToArray());
        //    foreach(var guid in guids)
        //    {
        //        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        //        if (groupAssets.Any(ga => ga.AssetPath == assetPath)) //已存在
        //            continue;
        //        string assetPathLower = assetPath.ToLower();
        //        //管理检查
        //        if (this.IsMatchedAssetPathLower(assetPathLower)) 
        //        {
        //            groupAssets.Add(new AssetPathAndGuid(assetPath, assetPathLower, guid));
        //        }
        //    }

        //}

        /// <summary>
        /// 尝试获取一个给定的路径是否是一个变体路径
        /// </summary>
        /// <param name="assetPath">路径（原始）</param>
        /// <param name="assetPathLower">路径（小写化）</param>
        /// <param name="variant">变体名称</param>
        /// <param name="sourceAssetPath">根据变体推导出的 根源 资产的路径（不小写化）</param>
        /// <returns>如果给定的路径确实符合一个变体规则，则返回true</returns>
        public virtual bool TryGetVariant(string assetPath, string assetPathLower, out string variant, out string sourceAssetPath)
        {
            int assetPathLowerLength = assetPathLower.Length;
            //首先我们来过一遍 AssetVariants 配置
            for (int i = 0; i < m_Config.AssetVariants.Count; i++)
            {
                var rule = m_Config.AssetVariants[i];
                for (int j = 0; j < rule.Variants.Count; j++)
                {
                    if (rule.Variants[j].AssetPath == assetPathLower)
                    {
                        //嚯找到了
                        variant = rule.Variants[j].Variant;
                        sourceAssetPath = rule.SourceAssetPath;
                        return true;
                    }
                    if (rule.Variants[j].AssetPath.Length > assetPathLowerLength)
                        break; //因为标准化阶段对AssetPath做了从小到大的排序，所以一旦这玩意比它大了之后后面的就都比它大了，就不可能再相等了
                }
            }

            //然后是 FolderVariants 
            for (int i = 0; i < m_Config.FolderVariants.Count; i++)
            {
                var rule = m_Config.FolderVariants[i];
                for (int j = 0; j < rule.Variants.Count; j++)
                {
                    if (rule.Variants[j].FolderPath.Length < assetPathLowerLength)
                    {
                        if (assetPathLower.StartsWith(rule.Variants[j].FolderPath))
                        {
                            //命中了规则
                            string variant_folder = rule.Variants[j].FolderPath;
                            variant = rule.Variants[j].Variant;
                            sourceAssetPath = rule.SourceFolderPath + assetPath.Substring(variant_folder.Length, assetPath.Length - variant_folder.Length);
                            return true;
                        }
                    }
                    else
                        break;
                }
            }

            variant = default;
            sourceAssetPath = default;
            return false;
        }

    }
}
