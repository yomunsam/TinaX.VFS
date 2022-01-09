using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TinaX.Core.Helper.Platform;
using TinaX.Core.Platforms;
using TinaX.VFS.Utils;
using TinaXEditor.Core.Helper.Platform;
using TinaXEditor.VFS.AssetBuilder.Discoverer;
using TinaXEditor.VFS.Packages.io.nekonya.tinax.vfs.Editor.Scripts.Utils;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Build.Pipeline;

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
            AssetBundleOutputFolder = EditorVFSUtils.GetProjectAssetBundleOutputPath(PlatformHelper.GetName(XRuntimePlatform.Unknow));
        }

        public UnityEditor.BuildTarget BuildTarget { get; set; }
        public UnityEditor.BuildTargetGroup BuildTargetGroup { get; set; }
        public XRuntimePlatform BuildPlatform { get; set; } = XRuntimePlatform.Unknow;

        public string AssetBundleOutputFolder { get; set; }

        //public AssetBuilder SetBuildPlatform(XRuntimePlatform platform)
        //{
        //    this.BuildPlatform = platform;
        //    this.BuildTarget = EditorPlatformHelper.GetBuildTarget(platform);
        //    this.BuildTargetGroup = EditorPlatformHelper.GetBuildTargetGroup(platform);
        //    AssetBundleOutputFolder = EditorVFSUtils.GetProjectAssetBundleOutputPath(PlatformHelper.GetName(platform));
        //    return this;
        //}

        //public AssetBuilder SetBuildTarget(UnityEditor.BuildTarget buildTarget)
        //{
        //    this.BuildTarget = buildTarget;
        //    return this;
        //}


        /// <summary>
        /// 执行资产构建
        /// </summary>
        //public async Task BuildAsync()
        //{
        //    if (!Directory.Exists(AssetBundleOutputFolder))
        //        Directory.CreateDirectory(AssetBundleOutputFolder);

        //    var assetBundles = await m_AssetDiscoverer.GetUnityAssetBundleBuildsAsync();
        //    var buildContent = new BundleBuildContent(assetBundles);
        //    var buildParams = new BundleBuildParameters(this.BuildTarget, this.BuildTargetGroup, this.AssetBundleOutputFolder);
        //    buildParams.UseCache = false;
        //    buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;
        //    //buildParams.WriteLinkXML = true;

        //    var exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out var results);
        //    if(exitCode >= ReturnCode.Success)
        //    {
        //        var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
        //        manifest.SetResults(results.BundleInfos);
        //        File.WriteAllText(Path.Combine(AssetBundleOutputFolder, "manifest"), manifest.ToString());
        //    }
            
        //}

        /// <summary>
        /// 在Assets的mete文件中标记AssetBundle（也就是会在编辑器右下角见到的那个）
        /// </summary>
        /// <returns></returns>
        //public Task MarkAssetsAsync()
        //{
        //    foreach(var item in m_AssetDiscoverer.GetAssetQueryResults())
        //    {
        //        //AssetImporter这个只能在主线程用
        //        var assetImporter = AssetImporter.GetAtPath(item.AssetPath);
        //        if(assetImporter != null && (assetImporter.assetBundleName != item.AssetBundleName || assetImporter.assetBundleVariant != item.VariantName))
        //        {
        //            assetImporter.SetAssetBundleNameAndVariant(item.AssetBundleName, item.VariantName);
        //        }
        //    }
        //    return Task.CompletedTask;
        //}

        public Task CopyAssetBundleToVirtualSpace()
        {
            string platformName = PlatformHelper.GetName(this.BuildPlatform);
            string projectVSpacePath = VFSUtils.GetProjectVirtualSpacePath();
            string mainPackageAbRootFolderInVSpace = VFSUtils.GetMainPackageAssetBundleRootFolder(projectVSpacePath, platformName);
            Dictionary<string, string> expansionPacksAbRootFolderInVSpace = new Dictionary<string, string>();

            //void CopyToMainPack(string fileName, string sourcePath)
            //{
            //    if(File.Exists(sourcePath))
            //    {
            //        var targetFilePath = Path.Combine(mainPackageAbRootFolderInVSpace, fileName);
            //        var targetFileFolder = Path.GetDirectoryName(targetFilePath);
            //        if (!Directory.Exists(targetFileFolder))
            //            Directory.CreateDirectory(targetFileFolder);

            //        if (File.Exists(targetFilePath))
            //            File.Delete(targetFilePath);
            //        File.Copy(sourcePath, targetFilePath);
            //    }
            //}

            void CopyToExpansionPack(string fileName, string sourcePath, string packageName)
            {
                if (File.Exists(sourcePath))
                {
                    string packageAbRootFolderInVSpace;
                    if(!expansionPacksAbRootFolderInVSpace.TryGetValue(packageName, out packageAbRootFolderInVSpace))
                    {
                        packageAbRootFolderInVSpace = VFSUtils.GetExpansionPackageAssetBundleRootFolder(projectVSpacePath, platformName, packageName);
                        expansionPacksAbRootFolderInVSpace.Add(packageName, packageAbRootFolderInVSpace);
                    }

                    var targetFilePath = Path.Combine(packageAbRootFolderInVSpace, fileName);
                    var targetFileFolder = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileFolder))
                        Directory.CreateDirectory(targetFileFolder);

                    if (File.Exists(targetFilePath))
                        File.Delete(targetFilePath);
                    File.Copy(sourcePath, targetFilePath);
                }
            }

            //foreach (var item in m_AssetDiscoverer.GetEditorAssetBundles())
            //{
            //    string fileName = VFSUtils.GetAssetBundleFileName(item.AssetBundleName, item.AssetBundleVariant);
            //    var filePath_outputFolder = Path.Combine(this.AssetBundleOutputFolder, fileName);
            //    if (item.ManagedByMainPack)
            //        CopyToMainPack(fileName, filePath_outputFolder);
            //    else
            //        CopyToExpansionPack(fileName, filePath_outputFolder, item.PackageName);
            //}

            return Task.CompletedTask;
        }
    }
}
