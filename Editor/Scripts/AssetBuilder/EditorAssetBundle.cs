using System.Collections.Generic;
using UnityEditor;

namespace TinaXEditor.VFS.AssetBuilder
{
    public class EditorAssetBundle
    {
        public string AssetBundleName { get; set; }

        public string AssetBundleVariant { get; set; }

        public List<EditorAssetInfo> Assets { get; set; }

        public bool ManagedByMainPack { get; set; }

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
            if (this.Assets == null)
                this.Assets = new List<EditorAssetInfo>();

            List<string> assetNames = new List<string>(this.Assets.Count);
            List<string> addressableNames = new List<string>(this.Assets.Count);

            foreach(var asset in this.Assets)
            {
                assetNames.Add(asset.AssetPath);
                addressableNames.Add(asset.FileNameInAssetBundle);
            }
            result.assetNames = assetNames.ToArray();
            result.addressableNames = addressableNames.ToArray();

            return result;
        }
    }
}
