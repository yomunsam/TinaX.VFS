using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using Debug = UnityEngine.Debug;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{

    /// <summary>
    /// 构建AssetBundle 管线处理器
    /// </summary>
    public class BuildAssetBundlesAsyncHandler : IBuildAssetsAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.BuildAssetBundles;

        public async Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("构建AssetBundle");
            else
                Debug.Log("Build AssetBundles");

            var sw = Stopwatch.StartNew();
            
            //输出路径
            if(!Directory.Exists(context.AssetBundlesOutputFolder))
                Directory.CreateDirectory(context.AssetBundlesOutputFolder);

            var assetBundleBuilds = await context.AssetBundles.GetUnityAssetBundleBuildsAsync();
            var buildContent = new BundleBuildContent(assetBundleBuilds);
            var buildParams = new BundleBuildParameters(context.BuildArgs.BuildTarget, context.BuildArgs.BuildTargetGroup, context.AssetBundlesOutputFolder);
            buildParams.BundleCompression = context.BuildArgs.BundleCompression;
            buildParams.UseCache = context.BuildArgs.UseCache;

            var exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out var results);
            bool success = exitCode >= ReturnCode.Success;
            if (success)
            {
                context.BundleBuildResults = results;
                var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
                manifest.SetResults(results.BundleInfos);
                File.WriteAllText(Path.Combine(context.AssetBundlesOutputFolder, "manifest"), manifest.ToString());
            }


            sw.Stop();
            if (context.HansLog)
                Debug.LogFormat("    构建AssetBundle结束，共用时:{0}秒", sw.Elapsed.TotalSeconds.ToString("N3"));
            else
                Debug.LogFormat("    Build AssetBundles finish, use time: {0}s", sw.Elapsed.TotalSeconds.ToString("N3"));

            if (!success)
                context.Break();
        }
    }
}
