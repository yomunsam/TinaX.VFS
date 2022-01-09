using TinaX.VFS.ConfigAssets;
using TinaX.VFS.ConfigProviders;

namespace TinaXEditor.VFS.Scripts.ConfigProviders
{
    /// <summary>
    /// VFS 各级配置的提供者（编辑器下）
    /// 编辑器下的配置应该是对配置资产有直接引用关系的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEditorConfigProvider<TConfig, TConfigModel> : IConfigProvider<TConfigModel>
    {
        /// <summary>
        /// 获取我们需要的编辑器配置（有些配置的标准化操作，编辑器和Runtime下是有差异的）
        /// </summary>
        TConfig EditorConfiguration { get; }

        //VFSConfigAsset VFSConfigAsset { get; }
    }
}
