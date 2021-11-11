using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaXEditor.VFS.AssetBuilder.Discoverer;
using UnityEditor;
using UnityEditor.Build.Pipeline;

namespace TinaXEditor.VFS.AssetBuilder.Builder
{
    /// <summary>
    /// 资产构建器
    /// </summary>
    public class AssetBuilder
    {
        private readonly IAssetDiscoverer m_AssetDiscoverer;

        public AssetBuilder(IAssetDiscoverer assetDiscoverer)
        {
            this.m_AssetDiscoverer = assetDiscoverer;
        }



        /// <summary>
        /// 执行资产构建
        /// </summary>
        public async Task BuildAsync()
        {
            //Todo:代码是及其简化的，不能正式使用
            var output_dir = Path.Combine(Directory.GetCurrentDirectory(), "TinaX/VFS/Build");
            if (!Directory.Exists(output_dir))
                Directory.CreateDirectory(output_dir);

            var assetBundles = await m_AssetDiscoverer.GetUnityAssetBundleBuilds();
            var buildContent = new BundleBuildContent(assetBundles);
            var buildParams = new BundleBuildParameters(BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, output_dir);
            buildParams.UseCache = false;
            buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;

            ContentPipeline.BuildAssetBundles(buildParams, buildContent, out var results);
        }

    }
}
