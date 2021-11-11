namespace TinaXEditor.VFS.AssetBuilder
{
    public struct EditorAssetInfo
    {
        /// <summary>
        /// 与Editor工程完全一致的资产寻址路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 虚拟资产路径
        /// </summary>
        public string VirtualAssetPath;

        /// <summary>
        /// 变体名
        /// </summary>
        public string VariantName;

        /// <summary>
        /// AssetBundle中，这个资产的名字叫啥
        /// </summary>
        public string FileNameInAssetBundle;

    }
}
