namespace TinaX.VFS.LoadAssets
{
    public enum AssetLoadModeInEditor
    {
        /// <summary>
        /// 正常加载AssetBundle
        /// </summary>
        Normal                                      = 0,
        /// <summary>
        /// 覆写StreamingAssets中的Virtual Space路径（如改成AssetBundle构建结果目录）
        /// </summary>
        OverrideVirtualSpaceInStreamingAssetsPath   = 1,

        /// <summary>
        /// 通过UnityEditor.AssetDatabase 直接加载而不通过AssetBundle
        /// </summary>
        LoadViaAssetDatabase                        = 2,
    }
}
