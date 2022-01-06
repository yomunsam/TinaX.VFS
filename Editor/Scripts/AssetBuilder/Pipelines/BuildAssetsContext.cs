using TinaX;
using TinaX.VFS.ConfigTpls;
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
        public EditorExpansionPackManager? EditorExpansionPackManager;

        /// <summary>
        /// 资产查询器
        /// </summary>
        public IEditorAssetQuerier? AssetQuerier;



        /// <summary>
        /// 是否终断Pipeline的标记
        /// </summary>
        public bool BreakPipeline { get; set; } = false;

        /// <summary>
        /// 汉语Log
        /// </summary>
        public bool HansLog { get; set; } = false;


        public IXCore? XCore { get; set; }
    }
}
