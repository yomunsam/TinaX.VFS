using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        //------------私有字段----------------------------------------------------------------------------------------------------------------------
        private List<string> m_TargetFilePaths = new List<string>(); //存储存放在Virtual Space目录（包括扩展包）中的实际文件路径


        //------------公开属性----------------------------------------------------------------------------------------------------------------------
        public string HandlerName => HandlerNameConsts.CopyAssetBundleToVirtualSpace;

        //------------公开方法----------------------------------------------------------------------------------------------------------------------
        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("复制AssetBundle");
            else
                Debug.Log("Copy AssetBundles");

            m_TargetFilePaths.Clear();
            string platformName = PlatformHelper.GetName(context.BuildArgs.BuildPlatform);
            string mainPackAssetBundleRootFolderInProjectVirtualSpace = VFSUtils.GetMainPackageAssetBundleRootFolder(m_ProjectVirtualSpacePath, platformName);

            if (context.BuildArgs.ClearProjectVirtualSpaceFolder)
            {
                if (Directory.Exists(mainPackAssetBundleRootFolderInProjectVirtualSpace))
                {
                    Directory.Delete(mainPackAssetBundleRootFolderInProjectVirtualSpace, true);
                    Directory.CreateDirectory(mainPackAssetBundleRootFolderInProjectVirtualSpace);
                }
            }

            foreach (var assetBundleInfo in context.AssetBundles.AssetBundleInfos)
            {
                var sourceFilePath = Path.Combine(context.AssetBundlesOutputFolder, assetBundleInfo.AssetBundleFileName);
                if (assetBundleInfo.ManagedByMainPack)
                {
                    CopyToMainPack(assetBundleInfo.AssetBundleFileName, sourceFilePath, mainPackAssetBundleRootFolderInProjectVirtualSpace);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }


            //if(!context.BuildArgs.ClearProjectVirtualSpaceFolder)
            //{
            //    //移除文件目录中实际存在，但本次AssetBundle打包时没有的内容。
            //    var files = Directory.GetFiles(mainPackAssetBundleRootFolderInProjectVirtualSpace, "*", SearchOption.AllDirectories);
            //    foreach (var file in files)
            //    {
            //        if (!m_TargetFilePaths.Contains(file))
            //        {
            //            File.Delete(file);
            //        }
            //    }
            //}


            m_TargetFilePaths.Clear();
            return Task.CompletedTask;
        }


        //------------私有方法----------------------------------------------------------------------------------------------------------------------
        private void CopyToMainPack(string fileName, string sourceFilePath, string virtualSpaceAssetBundleRootPath)
        {
            if (File.Exists(sourceFilePath))
            {
                var targetFilePath = Path.Combine(virtualSpaceAssetBundleRootPath, fileName);
#if UNITY_EDITOR_WIN
                targetFilePath = targetFilePath.Replace('/', '\\');
#endif
                var targetFileFolder = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileFolder))
                    Directory.CreateDirectory(targetFileFolder);
                File.Copy(sourceFilePath, targetFilePath, true);
                m_TargetFilePaths.Add(targetFilePath);
            }
        }

    }
}
