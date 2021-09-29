using TinaX.VFS.BuildRules;
using TinaX.VFS.Groups;
using TinaX.VFS.Packages;

namespace TinaX.VFS.Querier
{
    /// <summary>
    /// 资产查询结果
    /// </summary>
    public struct AssetQueryResult
    {
        public string AssetPath { get; set; }
        public string AssetPathLower { get; set; }
        public string AssetExtension { get; set; } //资产的文件后缀名（比如说检查后缀名黑名单的时候会用到）
        public bool ManagedByMainPack { get; set; }//是否被主包管理的资产
        public VFSPackage ManagedPackage { get; set; } //该资产被哪个包管理
        public VFSGroup ManagedGroup { get; set; } //该资产被哪个组管理
        public string AssetBundleName { get; set; } //其他程序通过这个地址实际加载AssetBundle
        public string OriginalAssetBundleName { get; set; } //原始AssetBundle名称，指通过打包规则推断出的原始名称，比如说后续某个流程打算混淆AssetBundleName的话，改上面哪个，这儿还应该是原始名称不动

        public string FileNameInAssetBundle { get; set; } //这个资产在AssetBundle包内的文件名（实际加载使用） （Unity的Scriptable build pipline的文档的案例标题中把这个叫做File Name，但它的代码里又叫addressableNames）
        public string OriginalFileNameInAssetBundle { get; set; } //原始FileNameInAssetBundle值

        public bool IsVariant { get; set; } //原始AssetPath是一个变体资产
        public string VariantName { get; set; } //资产变体名（如果不是变体则留空，或者也可以设置为VFS全局配置里的默认变体名）
        public string VariantSourceAssetPath { get; set; } //根据变体还原推导出的AssetPath（计算路径打包规则啥的记得用这个）
        public string VariantSourceAssetPathLower { get; set; } //顺便得到一个小写路径呗

        public FolderBuildType BuildType { get; set; }

        public bool HideDirectoryStructure { get; set; } //混淆目录结构

        public bool Valid { get; set; } //是否是有效的查询（这个查询结果在初始化的时候设为true，然后如果某个流程检测出无效，就设为false）（有效的定义：加载程序拿着这个查询结果就可以去跑加载流程了）
    }
}
