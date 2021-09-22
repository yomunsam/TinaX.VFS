using TinaX.VFS.ConfigAssets;
using UnityEditor;

namespace TinaXEditor.VFS.ScriptableSingletons
{
    /// <summary>
    /// 编辑器下 临时持久存储关于VFS Options的编辑器状态的 类
    /// 参考：https://docs.unity3d.com/ScriptReference/ScriptableSingleton_1.html
    /// </summary>
    [FilePath("Meow/Test.meow", FilePathAttribute.Location.ProjectFolder)]
    public class EditorVFSOptionScriptableSingleton : ScriptableSingleton<EditorVFSOptionScriptableSingleton>
    {
        public VFSConfigAsset VFSConfigAsset { get; set; }
    }
}
