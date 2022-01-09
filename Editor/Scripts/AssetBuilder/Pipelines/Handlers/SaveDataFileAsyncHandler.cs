using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinaX.Core.Helper.Platform;
using TinaX.VFS.SerializableModels.BundleManifest;
using TinaX.VFS.Utils;
using TinaXEditor.VFS.Packages.io.nekonya.tinax.vfs.Editor.Scripts.Utils;
using TinaXEditor.VFS.SerializableModels;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using FileUtil = TinaX.Core.Utils.FileUtil;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    /// <summary>
    /// 保存数据文件
    /// </summary>
    public class SaveDataFileAsyncHandler : IBuildAssetsAsyncHandler
    {
        //------------固定字段----------------------------------------------------------------------------------------------------------------------
        private readonly string m_ProjectVirtualSpacePath = VFSUtils.GetProjectVirtualSpacePath();



        public string HandlerName => HandlerNameConsts.SaveDataFiles;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("保存数据文件.");
            else
                Debug.Log("Save data files.");

            var platformName = PlatformHelper.GetName(context.BuildArgs.BuildPlatform);

            //------保存配置文件--------------------------------------
            //主包配置和全局配置（VFSConfigTpl）
            var mainPackDataFolderInProjectVirtualSpace = VFSUtils.GetMainPackageDataFolder(m_ProjectVirtualSpacePath, platformName);
            if (!Directory.Exists(mainPackDataFolderInProjectVirtualSpace))
                Directory.CreateDirectory(mainPackDataFolderInProjectVirtualSpace);
            string vfsConfigTplFilePath = VFSUtils.GetVFSConfigModelFilePath(mainPackDataFolderInProjectVirtualSpace);
            string vfsConfigTplJson = JsonUtility.ToJson(context.VFSConfigModel!);
            File.WriteAllText(vfsConfigTplFilePath, vfsConfigTplJson, Encoding.UTF8);

            //Todo: 扩展包的配置文件


            //------AssetBundle索引和依赖--------------------------------------
            var mainPackageManifest = new VFSBundleManifestModel()
            {
                Version = 1,
            };
            var mainPackBundleList = new List<AssetBundleDetailModel>();
            foreach(var item in context.AssetBundles.AssetBundleInfos)
            {
                if (item.ManagedByMainPack)
                {
                    if(context!.BundleBuildResults!.BundleInfos.TryGetValue(item.AssetBundleFileName, out var details))
                    {
                        var bundleModel = new AssetBundleDetailModel
                        {
                            AssetBundleName = item.AssetBundleFileName,
                            Crc = details.Crc,
                            Dependencies = details.Dependencies,
                        };
                        //MD5
                        try
                        {
                            var filePath = Path.Combine(context.AssetBundlesOutputFolder, item.AssetBundleFileName);
                            bundleModel.MD5 = TinaX.Core.Utils.FileUtil.GetMD5(filePath, true);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogErrorFormat("Get AssetBundle file ( {0} ) md5 failed : {1}", item.AssetBundleFileName, ex.Message);
                        }

                        mainPackBundleList.Add(bundleModel);
                    }
                    else
                    {
#if TINAX_DEV
                        Debug.LogErrorFormat("AssetBundle {0} 没有找到构建结果信息！", item.AssetBundleFileName);
#endif
                    }
                }
            }

            //主包 - 写出manifest
            mainPackageManifest.Bundles = mainPackBundleList.ToArray();
            mainPackBundleList.Clear();
            var mainPackManifestJson = JsonUtility.ToJson(mainPackageManifest);
            var mainPackManifestFilePath = VFSUtils.GetVFSBundleManifestFilePath(mainPackDataFolderInProjectVirtualSpace);
            File.WriteAllText(mainPackManifestFilePath, mainPackManifestJson, Encoding.UTF8);


            //------项目中Assets的原始文件的Hash--------------------------------------
            var projectRootPath = Directory.GetCurrentDirectory();
            var assetHashList = new List<AssetsHashItemModel>(context.AssetBundles.AssetQueryResults.Count);
            foreach(var item in context.AssetBundles.AssetQueryResults)
            {
                var filePath = Path.Combine(projectRootPath, item.AssetPath);
                var md5 = FileUtil.GetMD5(filePath, true);
                assetHashList.Add(new AssetsHashItemModel 
                { 
                    AssetPath = item.AssetPath, 
                    Hash = md5 , 
                    Bundle = item.AssetBundleName,
                    BundleVariant = item.VariantName,
                    FileNameInAssetBundle = item.FileNameInAssetBundle
                });
            }
            var assetsHashJson = JsonUtility.ToJson(new AssetsHashModel { Items = assetHashList.ToArray() });
            assetHashList.Clear();
            var assetsHashFilePath = EditorVFSUtils.GetAssetsHashFilePath();
            var assetsHashFilePathDir = Path.GetDirectoryName(assetsHashFilePath);
            if(!Directory.Exists(assetsHashFilePathDir))
                Directory.CreateDirectory(assetsHashFilePathDir);
            File.WriteAllText(assetsHashFilePath, assetsHashJson, Encoding.UTF8);

            return Task.CompletedTask;
        }
    }
}
