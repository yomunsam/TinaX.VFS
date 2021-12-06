using System.Collections.Generic;
using System.Linq;
using TinaX.VFS.Packages;
using TinaXEditor.VFS.Querier;
using UnityEngine;

namespace TinaXEditor.VFS.AssetBuilder.Discoverer
{
    /// <summary>
    /// 资产发现器 基类
    /// </summary>
    public abstract class AssetDiscovererBase
    {
        /// <summary>
        /// [AssetBundle Name] -> [AssetBundle 变体] -> Object
        /// </summary>
        protected Dictionary<string, Dictionary<string, EditorAssetBundle>> m_AssetBundleInfoDict = new Dictionary<string, Dictionary<string, EditorAssetBundle>>();

        /// <summary>
        /// 数据应该是和上面这个玩意一致的
        /// </summary>
        protected List<EditorAssetBundle> m_AssetBundleInfoList = new List<EditorAssetBundle>();

        protected List<EditorAssetQueryResult> m_AssetQueryResults = new List<EditorAssetQueryResult>();


        protected virtual void UpdateAssetBundleInfo(ref EditorAssetQueryResult queryResult)
        {
            string assetPath = queryResult.AssetPath;

            // Try get assetBundleInfo
            EditorAssetBundle assetBundleInfo;

            if (!m_AssetBundleInfoDict.ContainsKey(queryResult.AssetBundleName))
                m_AssetBundleInfoDict.Add(queryResult.AssetBundleName, new Dictionary<string, EditorAssetBundle>());

            if(!m_AssetBundleInfoDict[queryResult.AssetBundleName].TryGetValue(queryResult.VariantName, out assetBundleInfo))
            {
                assetBundleInfo = new EditorAssetBundle
                {
                    AssetBundleName = queryResult.AssetBundleName,
                    AssetBundleVariant = queryResult.VariantName,
                    ManagedByMainPack = queryResult.ManagedByMainPack,
                };
                if(!queryResult.ManagedByMainPack)
                {
                    assetBundleInfo.PackageName = (queryResult.ManagedPackage as VFSExpansionPack).PackageName;
                }
                m_AssetBundleInfoDict[queryResult.AssetBundleName].Add(queryResult.VariantName, assetBundleInfo);
            }

            if (!m_AssetBundleInfoList.Contains(assetBundleInfo))
                m_AssetBundleInfoList.Add(assetBundleInfo);

            //Update Info
            if (assetBundleInfo.Assets == null)
                assetBundleInfo.Assets = new List<EditorAssetInfo>();

            if(!assetBundleInfo.Assets.Any(a => a.AssetPath == assetPath))
            {
                assetBundleInfo.Assets.Add(new EditorAssetInfo
                {
                    AssetPath = queryResult.AssetPath,
                    VirtualAssetPath = queryResult.VirtualAssetPath,
                    VariantName = queryResult.VariantName,
                    FileNameInAssetBundle = queryResult.FileNameInAssetBundle
                });
            }


        }


#if TINAX_DEV
        public void DebugPrint()
        {
            Debug.Log("下面打印一下发现的资产（AB）信息");
            foreach (var ab in m_AssetBundleInfoList)
            {
                Debug.LogFormat("[AB]{0}.{1}", ab.AssetBundleName, ab.AssetBundleVariant);
                if(ab.Assets != null && ab.Assets.Count > 0)
                {
                    foreach(var asset in ab.Assets)
                    {
                        Debug.LogFormat("    [<color=#66ccff>{0}</color>] Editor路径：<color=#ffffff>{1}</color> |    Filename in AB:<color=green> {2}</color>", asset.VirtualAssetPath, asset.AssetPath, asset.FileNameInAssetBundle);
                    }
                }
            }
        }
#endif
    }
}
