namespace TinaX.VFS.Const
{
    public static class VFSConst
    {
        public const string ServiceName = "TinaX.VFS";
        public const string DefaultConfigAssetName = "VFS";

        /// <summary>
        /// 默认AssetBundle Variant
        /// </summary>
        public const string DefaultAssetBundleVariant = ".xa";

        /// <summary>
        /// 全局强制忽略的文件扩展名
        /// </summary>
        public static string[] GlobalIgnoreExtensions =
        {
            ".cs",
            ".fs"
        };

        /// <summary>
        /// 全局强制忽略的路径项
        /// </summary>
        public static string[] GlobalIgnorePathItem =
        {
            "Editor",
        };
    }
}
