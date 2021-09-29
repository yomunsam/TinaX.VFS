namespace TinaX.VFS.Consts
{
    public static class VFSConsts
    {
        public const string ServiceName = "TinaX.VFS";
        public const string DefaultConfigAssetName = "VFS";

        /// <summary>
        /// 默认AssetBundle Variant
        /// </summary>
        public const string DefaultAssetBundleVariant = ".xa";

        /// <summary>
        /// VFS配置模板（VFSConfigTpl）生成的Json文件的名字，
        /// 按照约定它应该放在Virtual Space中主包的data目录下
        /// </summary>
        public const string VFSConfigJsonFileName = "vfs.conf.json";

        /// <summary>
        /// 固定资产构建Tag: Editor Only
        /// </summary>
        public const string BuildTag_EditorOnly = @"Editor";

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
        public static string[] GlobalIgnoreFolderName =
        {
            "Editor",
        };
    }
}
