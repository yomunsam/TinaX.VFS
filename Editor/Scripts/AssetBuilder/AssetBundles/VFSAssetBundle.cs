using System.Collections.Generic;
using TinaX.VFS.Utils;
using UnityEditor;

namespace TinaXEditor.VFS.AssetBuilder.AssetBundles
{
    /// <summary>
    /// 资产构建器 AssetBundle 对象
    /// </summary>
    public class VFSAssetBundle
    {
        public string AssetBundleName { get; set; }
        public string AssetBundleVariant { get; set; }

        /// <summary>
        /// AssetBundle文件名（纯文件名，不含目录
        /// </summary>
        public string AssetBundleFileName
        {
            get
            {
                if(_assetBundleFileName == null)
                    _assetBundleFileName = VFSUtils.GetAssetBundleFileName(AssetBundleName, AssetBundleVariant);
                return _assetBundleFileName;
            }
        }

        private string _assetBundleFileName;

        /// <summary>
        /// 该AssetBundle中存在的资产
        /// </summary>
        public readonly List<EditorAssetInfo> Assets = new List<EditorAssetInfo>();

        public bool ManagedByMainPack { get; set; } //主包中的资产

        /// <summary>
        /// If not mainpackage, expansion package name
        /// </summary>
        public string PackageName { get; set; }

        public AssetBundleBuild GetUnityAssetBundleBuild()
        {
            var result = new AssetBundleBuild
            {
                //因为Unity的Scriptable Build Pipeline居然tmd不支持变体，所以我们自己在这里瞎改改 
                //assetBundleName = this.AssetBundleName,
                assetBundleName = $"{this.AssetBundleName}.{this.AssetBundleVariant}",
                assetBundleVariant = this.AssetBundleVariant,
            };

            List<string> assetNames = new List<string>(this.Assets.Count);
            List<string> addressableNames = new List<string>(this.Assets.Count);

            foreach (var asset in this.Assets)
            {
                assetNames.Add(asset.ProjectAssetPath);
                addressableNames.Add(asset.FileNameInAssetBundle);
            }
            result.assetNames = assetNames.ToArray();
            result.addressableNames = addressableNames.ToArray();

            return result;
        }
    }
}
