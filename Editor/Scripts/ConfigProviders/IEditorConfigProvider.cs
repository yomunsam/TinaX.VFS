using TinaX.VFS.ConfigProviders;

namespace TinaXEditor.VFS.Scripts.ConfigProviders
{
    /// <summary>
    /// VFS 各级配置的提供者（编辑器下）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEditorConfigProvider<T> : IConfigProvider<T>
    {
        /// <summary>
        /// 获取我们需要的编辑器配置（有些配置的标准化操作，编辑器和Runtime下是有差异的）
        /// </summary>
        T EditorConfiguration { get; }

    }
}
