using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;


#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    public class MarkAssetBundleInfoAsyncHandler : IBuildAssetsAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.MarkAssetBundleInfo;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("标记AssetBundle信息 （显示在Inspector底部的信息）");
            else
                Debug.Log("Mark AssetBundle info. (Information displayed at the bottom of the Inspector)");

            if (!context.BuildArgs.MarkAssetBundleInfo)
            {
                if (context.HansLog)
                    Debug.Log("标记 AssetBundle信息的步骤被跳过");
                else
                    Debug.Log("The step of marking AssetBundle information is skipped");

                return Task.CompletedTask;
            }

            var sw = Stopwatch.StartNew();

            foreach(var item in context.AssetBundles.AssetQueryResults)
            {
                var assetImporter = AssetImporter.GetAtPath(item.AssetPath);
                if (assetImporter != null && (assetImporter.assetBundleName != item.AssetBundleName || assetImporter.assetBundleVariant != item.VariantName))
                {
                    assetImporter.SetAssetBundleNameAndVariant(item.AssetBundleName, item.VariantName);
                }
            }

            sw.Stop();
            if (context.HansLog)
                Debug.LogFormat("标记AssetBundle信息的步骤完成，共用时 {0} 秒", sw.Elapsed.TotalSeconds.ToString("N3"));
            else
                Debug.LogFormat("The step of marking AssetBundle information completed, took {0} seconds", sw.Elapsed.TotalSeconds.ToString("N3"));
            return Task.CompletedTask;
        }
    }
}
