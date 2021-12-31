using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;
using TinaXEditor.VFS.Querier;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Discoverer
{
    /// <summary>
    /// 项目资产发现器
    /// 范围为整个Unity工程
    /// </summary>
    public class ProjectAssetDiscoverer : AssetDiscovererBase, IAssetDiscoverer
    {
        private readonly IEditorAssetQuerier m_AssetQuerier;
        private readonly EditorMainPackage m_MainPackage;
        private readonly EditorExpansionPackManager? m_ExpansionPackManager;
        private readonly GlobalAssetConfigTpl m_GlobalAssetConfig;

        public ProjectAssetDiscoverer(IEditorAssetQuerier assetQuerier,
            EditorMainPackage mainPackage,
            EditorExpansionPackManager? expansionPackManager,
            GlobalAssetConfigTpl globalAssetConfig)
        {
            this.m_AssetQuerier = assetQuerier ?? throw new ArgumentNullException(nameof(assetQuerier));
            this.m_MainPackage = mainPackage ?? throw new ArgumentNullException(nameof(mainPackage));
            this.m_ExpansionPackManager = expansionPackManager;
            this.m_GlobalAssetConfig = globalAssetConfig ?? throw new ArgumentNullException(nameof(globalAssetConfig));
        }

        /// <summary>
        /// 是否执行过至少一次收集任务
        /// </summary>
        public bool Collected { get; private set; } = false;

        

        /// <summary>
        /// 收集可被管理的资产
        /// </summary>
        /// <returns></returns>
        public async Task CollectManageableAssetsAsync()
        {
            var guids = AssetDatabase.FindAssets("", new string[] { "Assets/" });
#if TINAX_DEV
            Debug.LogFormat("guids count:{0}", guids.Length);
#endif
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                //忽略文件夹、C#代码等内容
                if(type == typeof(UnityEditor.MonoScript)
                    || type == typeof(UnityEditor.DefaultAsset))
                {
                    continue;
                }

                //在查询器中查询该资产
                var query_result = m_AssetQuerier.QueryAsset(assetPath, m_MainPackage, m_ExpansionPackManager, m_GlobalAssetConfig);
                if (!query_result.Valid)
                    continue;

                //整理信息
                UpdateAssetBundleInfo(ref query_result);

                //存储信息
                m_AssetQueryResults.Add(query_result);

            }

            await Task.CompletedTask;
            Collected = true;
        }

        public Task<AssetBundleBuild[]> GetUnityAssetBundleBuildsAsync()
        {
            var result = new List<AssetBundleBuild>(this.m_AssetBundleInfoList.Count);
            foreach(var item in this.m_AssetBundleInfoList)
            {
                result.Add(item.GetUnityAssetBundleBuild());
            }
            return Task.FromResult(result.ToArray());
        }

        public EditorAssetQueryResult[] GetAssetQueryResults()
        {
            return m_AssetQueryResults.ToArray();
        }

        public EditorAssetQueryResult[] GetMainPackAssetQueryResults()
        {
            return m_AssetQueryResults.Where(qr => qr.ManagedByMainPack).ToArray();
        }

        public EditorAssetQueryResult[] GetExpansionPackAssetQueryResults(string packageName)
        {
            var query = from qr in m_AssetQueryResults
                        where qr.ManagedByMainPack == false && (qr.ManagedPackage as VFSExpansionPack).PackageName == packageName
                        select qr;
            return query.ToArray();
        }


        public EditorAssetBundle[] GetEditorAssetBundles()
            => m_AssetBundleInfoList.ToArray();
    }
}
