namespace TinaX.VFS.Querier.Pipelines.Handlers
{
    /// <summary>
    /// 查询资产的Pipeline流程：隐藏AssetBundle的目录结构
    /// 
    /// </summary>
    public class HideAssetBundleDirectoryStructure : IQueryAssetHandler
    {
        public string HandlerName => QueryAssetHandlerNameConsts.HideDirectoryStructure;

        public void QueryAsset(ref QueryAssetContext context, ref AssetQueryResult result)
        {
            if (!result.Valid)
                return; //无效的资产不用管了

            if (result.HideDirectoryStructure)
            {
                //我们的规则还是用原始assetBundleName（不包含后缀）计算MD5，这儿规则和TinaX 6一样
                string assetbundle_md5_32 = result.OriginalAssetBundleName.GetMD5(true, false);
                //result.AssetBundleName = string.Format("{0}/{1}", assetbundle_md5_32[0..2], assetbundle_md5_32[8..16]);
                result.AssetBundleName = string.Format("{0}/{1}", assetbundle_md5_32.Substring(0, 2), assetbundle_md5_32.Substring(8, 16)); //截至写这段代码的时候，Unity不能用切片语法糖（Range）写法，辣鸡
                result.FileNameInAssetBundle = result.VirtualAssetPathLower.GetMD5(true, true); //TinaX 6里没有这一条，这是VFS 7用了Unity Scriptable Build Pipeline之后才能自定义的东西
            }
        }
    }
}
