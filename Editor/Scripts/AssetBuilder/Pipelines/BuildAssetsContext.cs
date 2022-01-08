using TinaX;
using TinaX.VFS.ConfigTpls;
using TinaXEditor.VFS.AssetBuilder.AssetBundles;
using TinaXEditor.VFS.AssetBuilder.Discoverer;
using TinaXEditor.VFS.Packages;
using TinaXEditor.VFS.Packages.Managers;
using TinaXEditor.VFS.Querier;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines
{
    /// <summary>
    /// 构建资产的上下文
    /// </summary>
    public class BuildAssetsContext
    {
        public BuildAssetsContext(BuildAssetsArgs args)
        {
            this.BuildArgs = args;
        }

        /// <summary>
        /// 资产的全局配置
        /// </summary>
        public GlobalAssetConfigTpl? GlobalConfig;

        /// <summary>
        /// 主包 编辑器对象
        /// </summary>
        public EditorMainPackage? MainPackage;

        /// <summary>
        /// 扩展包 管理器 对象
        /// </summary>
        public EditorExpansionPackManager? ExpansionPackManager;

        /// <summary>
        /// 资产查询器
        /// </summary>
        public IEditorAssetQuerier? AssetQuerier;

        /// <summary>
        /// 资产发现器
        /// </summary>
        public IAssetDiscoverer? AssetDiscoverer;

        /// <summary>
        /// 待构建的AssetBundle信息
        /// </summary>
        public VFSAssetBundles AssetBundles = new VFSAssetBundles();

        public BuildAssetsArgs BuildArgs;

        public string AssetBundlesOutputFolder { get; set; } = string.Empty;

        /// <summary>
        /// 是否终断Pipeline的标记
        /// </summary>
        public bool BreakPipeline { get; set; } = false;

        /// <summary>
        /// 汉语Log
        /// </summary>
        public bool HansLog { get; set; } = false;

    }
}
