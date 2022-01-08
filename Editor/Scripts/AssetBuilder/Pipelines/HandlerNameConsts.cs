#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines
{
    public static class HandlerNameConsts
    {

        //------------准备阶段------------------------------------------------------------------------

        /// <summary>
        /// 准备VFS编辑器对象
        /// </summary>
        public const string PrepareVFSEditorObjects = @"PrepareVFSEditorObjects";

        /// <summary>
        /// 发现资产（搜寻将要被打包的资产）
        /// </summary>
        public const string DiscoverAssets = @"DiscoverAssets";

        /// <summary>
        /// 标记AssetBundle信息
        /// （就是可以在编辑器右下角看到的那个信息，其实没啥用，VFS不使用那个信息打包）
        /// </summary>
        public const string MarkAssetBundleInfo = @"MarkAssetBundleInfo";

        //------------打包AB------------------------------------------------------------------------

        /// <summary>
        /// 构建AssetBundle
        /// </summary>
        public const string BuildAssetBundles = @"BuildAssetBundles";


        //------------准备Virtul Space-------------------------------------------------------------

        public const string CopyAssetBundleToVirtualSpace = @"CopyAssetBundleToVirtualSpace";

        public const string SaveDataFiles = @"SaveDataFiles";

    }
}
