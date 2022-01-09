#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TinaX.VFS.LoadAssets
{
    /// <summary>
    /// VFS 在编辑器下的加载模式
    /// </summary>
    public static class VFSLoadModeInEditor
    {
        /// <summary>
        /// 资产加载路径
        /// </summary>
        public static AssetLoadModeInEditor LoadMode
        {
            get
            {
#if UNITY_EDITOR
                return ScriptableSingleton<VFSLoadModeInEditorStatus>.instance.LoadMode;
#else
                return AssetLoadModeInEditor.Normal;
#endif
            }
        }

        public static string OverrideVirtualSpaceInStreamingAssetsPath
        {
            get
            {
#if UNITY_EDITOR
                return ScriptableSingleton<VFSLoadModeInEditorStatus>.instance.OverrideVirtualSpaceInStreamingAssetsPath;
#else
                return string.Empty;
#endif
            }
        }


    }



#if UNITY_EDITOR
    public class VFSLoadModeInEditorStatus : ScriptableSingleton<VFSLoadModeInEditorStatus>
    {
        public AssetLoadModeInEditor LoadMode = AssetLoadModeInEditor.LoadViaAssetDatabase;
        public string OverrideVirtualSpaceInStreamingAssetsPath;
    }
#endif
}
