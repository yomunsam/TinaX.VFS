using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinaX.VFS.Packages;
using TinaXEditor.VFS.Querier;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.AssetBundles
{
    /// <summary>
    /// 待构建的AssetBundles
    /// </summary>
    public class VFSAssetBundles
    {
        private readonly List<VFSAssetBundle> m_AssetBundleInfoList = new List<VFSAssetBundle>();
        private readonly List<EditorAssetQueryResult> m_AssetQueryResults = new List<EditorAssetQueryResult>();

        public int AssetBundleCount => m_AssetBundleInfoList.Count;

        public List<EditorAssetQueryResult> AssetQueryResults => m_AssetQueryResults;
        public List<VFSAssetBundle> AssetBundleInfos => m_AssetBundleInfoList;

        public void RegisterByQueryResult(in EditorAssetQueryResult queryResult)
        {
            var _assetBundleName = queryResult.AssetBundleName;
            var _assetBundleVariant = queryResult.VariantName;
            var _projectAssetPath = queryResult.AssetPath;

            var assetBundleInfo = m_AssetBundleInfoList.FirstOrDefault(ab => ab.AssetBundleName == _assetBundleName && ab.AssetBundleVariant == _assetBundleVariant);
            if(assetBundleInfo == null)
            {
                assetBundleInfo = new VFSAssetBundle
                {
                    AssetBundleName = _assetBundleName,
                    AssetBundleVariant = _assetBundleVariant,
                    ManagedByMainPack = queryResult.ManagedByMainPack,
                };
                if (!assetBundleInfo.ManagedByMainPack)
                    assetBundleInfo.PackageName = (queryResult.ManagedPackage as VFSExpansionPack)!.PackageName;
                m_AssetBundleInfoList.Add(assetBundleInfo);
                m_AssetQueryResults.Add(queryResult);
            }
            else
            {
                if(!assetBundleInfo.Assets.Any(a => a.ProjectAssetPath == _projectAssetPath))
                {
                    assetBundleInfo.Assets.Add(new EditorAssetInfo
                    {
                        ProjectAssetPath = queryResult.AssetPath,
                        VirtualAssetPath = queryResult.VirtualAssetPath,
                        VariantName = queryResult.VariantName,
                        FileNameInAssetBundle = queryResult.FileNameInAssetBundle
                    });
                    m_AssetQueryResults.Add(queryResult);
                }
            }

        }

        public Task<AssetBundleBuild[]> GetUnityAssetBundleBuildsAsync()
        {
            var result = new List<AssetBundleBuild>(m_AssetBundleInfoList.Count);
            var syncRoot = (result as ICollection).SyncRoot;
            Parallel.ForEach(m_AssetBundleInfoList, item =>
            {
                var build = item.GetUnityAssetBundleBuild(); //这个操作估计也不怎么耗时，所以实际也说不好到底是都放在一个线程里写比较快还是并行比较快，有空了测一下试试看
                lock(syncRoot)
                {
                    result.Add(build);
                }
            });

            return Task.FromResult(result.ToArray());
        }


#if TINAX_DEV
        /// <summary>
        /// 打印本对象中管理的AssetBundle信息
        /// </summary>
        public void PrintAssetBundles()
        {
            Debug.LogFormat("AssetBundles, 长度 {0} ...", m_AssetBundleInfoList.Count);
            foreach (var ab in m_AssetBundleInfoList)
            {
                Debug.LogFormat("[AB]{0}.{1}", ab.AssetBundleName, ab.AssetBundleVariant);
                if (ab.Assets != null && ab.Assets.Count > 0)
                {
                    foreach (var asset in ab.Assets)
                    {
                        Debug.LogFormat("    [<color=#66ccff>{0}</color>] Editor路径：<color=#ffffff>{1}</color> |    Filename in AB:<color=green> {2}</color>", asset.VirtualAssetPath, asset.ProjectAssetPath, asset.FileNameInAssetBundle);
                    }
                }
            }
        }
#endif
    }
}
