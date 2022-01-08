using TinaX.Core.Platforms;
using TinaXEditor.Core.Helper.Platform;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder
{
    /// <summary>
    /// 调用 BuildAssets 时的附带参数
    /// </summary>
    public class BuildAssetsArgs
    {
        public BuildAssetsArgs(XRuntimePlatform platform)
        {
            BuildPlatform = platform;
            this.BuildTarget = EditorPlatformHelper.GetBuildTarget(platform);
            this.BuildTargetGroup = EditorPlatformHelper.GetBuildTargetGroup(platform);
        }



        public XRuntimePlatform BuildPlatform { get; set; }
        public UnityEditor.BuildTarget BuildTarget { get; set; }
        public UnityEditor.BuildTargetGroup BuildTargetGroup { get; set; }

        /// <summary>
        /// 标记AssetBundle信息
        /// （就是可以在编辑器右下角看到的那个信息，其实没啥用，VFS不使用那个信息打包）
        /// </summary>
        public bool MarkAssetBundleInfo { get; set; } = false;
        public bool ClearProjectVirtualSpaceFolder { get; set; } = true;


        public bool UseCache { get; set; } = false;
        public UnityEngine.BuildCompression BundleCompression { get; set; } = UnityEngine.BuildCompression.LZ4;
    }
}
