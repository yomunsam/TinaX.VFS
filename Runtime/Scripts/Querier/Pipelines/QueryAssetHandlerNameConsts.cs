namespace TinaX.VFS.Querier.Pipelines
{
    /// <summary>
    /// 定义VFS内置的查询资产的Handler的名称
    /// </summary>
    public static class QueryAssetHandlerNameConsts
    {
        /// <summary>
        /// 通过全局忽略规则来过滤加载资产信息
        /// </summary>
        public const string FilterByGlobalIgnoreRule = @"FilterByGlobalIgnoreRule";
        public const string QueryFromMainPackage = @"QueryFromMainPackage";
        public const string QueryFromExpansionPacks = @"QueryFromExpansionPacks";

        /// <summary>
        /// 隐藏目录结构
        /// </summary>
        public const string HideDirectoryStructure = @"HideDirectoryStructure";
    }
}
