namespace TinaX.VFS.Pipelines.LoadAsset
{
    public static class HandlerNameConsts
    {
        //------------ 查询处理 -------------------------------------------------

        /// <summary>
        /// 查询资产
        /// </summary>
        public const string QueryAsset = @"QueryAsset";

        /// <summary>
        /// 检查已加载的资产
        /// </summary>
        public const string CheckLoadedAsset = @"CheckLoadedAsset";


        //------------ 加载AssetBundle -------------------------------------------------

        public const string PrepareAssetBundleLoadInfo = @"PrepareAssetBundleLoadInfo"; //准备加载信息，如加载路径等等

        /// <summary>
        /// 准备AssetBundle加载器
        /// </summary>
        public const string PrepareAssetBundleLoader = @"PrepareAssetBundleLoader"; //就是塞个加载器进来，比如说要实现WebVFS的话，就在后面插一个Handler把加载器替换掉
    }
}
