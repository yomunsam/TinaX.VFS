using System.Threading;
using System.Threading.Tasks;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.ConfigProviders;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Consts;
using TinaXEditor.Core;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    /// <summary>
    /// 准备 VFS 编辑器对象 处理器
    /// </summary>
    public class PrepareVFSEditorObjectsHandler : IBuildAssetsAsyncHandler
    {
        public string HandlerName => HandlerNameConsts.PrepareVFSEditorObjects;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
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

            //全局配置资产
            //string jsonStr = JsonUtility.ToJson(configAsset.GlobalAssetConfig); //用JSON做个深拷贝
            //var globalConfigProvider = new GlobalAssetConfigProvider(JsonUtility.FromJson<GlobalAssetConfigTpl>(jsonStr));
            //globalConfigProvider.Standardize(); //对全局配置


            return Task.CompletedTask;
        }
    }
}
