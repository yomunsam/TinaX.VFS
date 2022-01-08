using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TinaXEditor.VFS.AssetBuilder.AssetBundles;
using TinaXEditor.VFS.Models;
using TinaXEditor.VFS.Querier;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace TinaXEditor.VFS.AssetBuilder.Discoverer
{
    /// <summary>
    /// 项目全局的资产发现器
    /// </summary>
    public class GlobalAssetDiscoverer : IAssetDiscoverer
    {

        /// <summary>
        /// 在整个项目的全局范围内发现可被管理的资产
        /// </summary>
        /// <param name="vfsAssetBundles"></param>
        public void DiscoverAssets(VFSAssetBundles vfsAssetBundles, IEditorAssetQuerier assetQuerier)
        {
            /*
             * 这里的做法是：获取到整个工程中所有的资产文件，再通过查询器判断是否是有效的可被管理的资产
             * 
             * 通过查询器这个做法其实效率不如直接从 VFS配置资产中获取数据，但好处是统一了逻辑，方便业务层面扩展VFS的行为。
             */
#if TINAX_DEV
            Debug.Log("在项目全局范围内发现可被管理的资产");
#endif
            var _monoScriptType = typeof(UnityEditor.MonoScript);
            var _defaultAssetType = typeof(UnityEditor.DefaultAsset);

            var sw = new Stopwatch();
            sw.Start();
            var guids = AssetDatabase.FindAssets("", new string[] { "Assets/" });
            var assetPaths = new List<AssetPathAndGuid>();
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                //忽略文件夹， C#等内容
                if (assetType == _monoScriptType || assetType == _defaultAssetType)
                    continue;

                assetPaths.Add(new AssetPathAndGuid(assetPath, guid));

                //因为Unity的API基本都不能在多线程中调用，所以这部分涉及到AssetDatabase的，我们单独弄一个仅在主线程中的for循环尽快处理完，剩下的可以做并行计算
            }

#if TINAX_DEV
            Debug.LogFormat("列出工程中所有资产共用时 {0} 秒，共 {1} 条资产记录", sw.Elapsed.TotalSeconds.ToString("N3"), assetPaths.Count);
#endif
            Parallel.ForEach(assetPaths, assetPath => //剩下和Unity Api无关的东西，可以并行处理了
            {
                //查询资产
                var queryResult = assetQuerier.QueryAsset(assetPath.AssetPath);
                if (queryResult.Valid) //有效的资产
                {
                    lock(vfsAssetBundles)
                    {
                        vfsAssetBundles.RegisterByQueryResult(in queryResult);
                    }
                }
            });

            sw.Stop();

#if TINAX_DEV
            Debug.LogFormat("发现资产完毕，总用时 {0} 秒，发现AssetBundle数量 {1}", sw.Elapsed.TotalSeconds.ToString("N3"), vfsAssetBundles.AssetBundleCount);
#endif
        }
    }
}
