using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TinaX.Core.Helper.Platform;
using TinaX.VFS.Utils;
using Debug = UnityEngine.Debug;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    /// <summary>
    /// 构建资产流程 复制AssetBundle到Virtual Space
    /// </summary>
    public class CopyAssetBundleToVirtualSpaceAsyncHandler : IBuildAssetsAsyncHandler
    {
        //------------固定字段----------------------------------------------------------------------------------------------------------------------
        private readonly string m_ProjectVirtualSpacePath = VFSUtils.GetProjectVirtualSpacePath();

        //------------构造方法----------------------------------------------------------------------------------------------------------------------
        public CopyAssetBundleToVirtualSpaceAsyncHandler()
        {
        }

        //------------私有方法----------------------------------------------------------------------------------------------------------------------


        public string HandlerName => HandlerNameConsts.CopyAssetBundleToVirtualSpace;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("复制AssetBundle");
            else
                Debug.Log("Copy AssetBundles");

            string platformName = PlatformHelper.GetName(context.BuildArgs.BuildPlatform);
            string mainPackAssetBundleRootFolderInProjectVirtualSpace = VFSUtils.GetMainPackageAssetBundleRootFolder(m_ProjectVirtualSpacePath, platformName);

            foreach(var assetBundleInfo in context.AssetBundles.AssetBundleInfos)
            {
                string assetBundleFileName = VFSUtils.GetAssetBundleFileName(assetBundleInfo.AssetBundleName, assetBundleInfo.AssetBundleVariant);
                var sourceFilePath = Path.Combine(context.AssetBundlesOutputFolder, assetBundleFileName);
                if (assetBundleInfo.ManagedByMainPack)
                {
                    CopyToMainPack(assetBundleFileName, sourceFilePath, mainPackAssetBundleRootFolderInProjectVirtualSpace);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            
            return Task.CompletedTask;
        }


        private void CopyToMainPack(string fileName, string sourceFilePath, string virtualSpaceAssetBundleRootPath)
        {
            if (File.Exists(sourceFilePath))
            {
                var targetFilePath = Path.Combine(virtualSpaceAssetBundleRootPath, fileName);
                var targetFileFolder = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileFolder))
                    Directory.CreateDirectory(targetFileFolder);
                File.Copy(sourceFilePath, targetFilePath, true);
            }
        }

    }
}
