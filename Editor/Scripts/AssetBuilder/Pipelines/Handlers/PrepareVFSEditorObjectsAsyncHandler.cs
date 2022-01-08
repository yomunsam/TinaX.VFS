using System.Threading;
using System.Threading.Tasks;
using TinaX.Core.Helper.Platform;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Consts;
using TinaX.VFS.Utils;
using TinaXEditor.Core;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.ConfigProviders;
using TinaXEditor.VFS.Packages.io.nekonya.tinax.vfs.Editor.Scripts.Utils;
using TinaXEditor.VFS.Querier;
using TinaXEditor.VFS.Querier.Pipelines;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    /// <summary>
    /// 准备 VFS 编辑器对象 处理器
    /// </summary>
    public class PrepareVFSEditorObjectsAsyncHandler : IBuildAssetsAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.PrepareVFSEditorObjects;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("准备VFS编辑器对象");
            else
                Debug.Log("Prepare the VFS editor objects.");

            //加载工程中的配置资产
            var configAsset = EditorConfigAsset.GetConfig<VFSConfigAsset>(VFSConsts.DefaultConfigAssetName);
            if(configAsset == null)
            {
                //报错
                if (context.HansLog)
                    Debug.LogError("构建资产失败：未能在工程中加载得到VFS配置资产，请检查是否存在有效的配置资产。");
                else
                    Debug.LogError("Failed to build assets: failed to load the VFS configuration asset in the project, please check if a valid configuration asset exists.");

                return Task.CompletedTask;
            }
            var configTpl = VFSConfigUtils.MapToVFSConfigTpl(configAsset);
            context.VFSConfigTpl = configTpl;

            //主包
            var mainPackageConfigProvider = new EditorMainPackageConfigProvider(configTpl.MainPackage);
            mainPackageConfigProvider.Standardize(); //标准化配置
            context.MainPackage = new EditorMainPackage(mainPackageConfigProvider);
            context.MainPackage.Initialize(); //初始化

            //Todo: 扩展包
            context.ExpansionPackManager = new Packages.Managers.EditorExpansionPackManager();

            //资产查询器
            var querier = new EditorAssetQuerier(EditorQueryAssetPipelineDefault.CreateDefault(), context.MainPackage, context.ExpansionPackManager, configTpl.GlobalAssetConfig);
            context.AssetQuerier = querier;


            //AssetBundles 输出位置
            context.AssetBundlesOutputFolder = EditorVFSUtils.GetProjectAssetBundleOutputPath(PlatformHelper.GetName(context.BuildArgs.BuildPlatform));

            return Task.CompletedTask;
        }
    }
}
