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
        /// <summary>
        /// 虚拟资产路径（也就是LoadPath）
        /// 由于变体等功能的存在，Runtime下的LoadPath可能并不会与工程的“Assets/”目录结构完全一一对应
        /// Runtime下没有与Editor完全一致的AssetPath概念
        /// </summary>
        public string VirtualAssetPath { get; set; }

        public string VirtualAssetPathLower { get; set; }
        
        /// <summary>
        /// 变体名，请传入变体或默认变体
        /// </summary>
        public string VariantName { get; set; }
        public string AssetExtension { get; set; } //资产的文件后缀名（比如说检查后缀名黑名单的时候会用到）
        public bool ManagedByMainPack { get; set; }//是否被主包管理的资产
        public VFSPackage ManagedPackage { get; set; } //该资产被哪个包管理
        public VFSGroup ManagedGroup { get; set; } //该资产被哪个组管理

        public string AssetBundleName { get; set; } //其他程序通过这个地址实际加载AssetBundle（这里是没有后缀名或者说变体的）
        public string OriginalAssetBundleName { get; set; } //原始AssetBundle名称，指通过打包规则推断出的原始名称，比如说后续某个流程打算混淆AssetBundleName的话，改上面哪个，这儿还应该是原始名称不动


        public string FileNameInAssetBundle { get; set; } //这个资产在AssetBundle包内的文件名（实际加载使用） （Unity的Scriptable build pipline的文档的案例标题中把这个叫做File Name，但它的代码里又叫addressableNames）
        public string OriginalFileNameInAssetBundle { get; set; } //原始FileNameInAssetBundle值

        public FolderBuildType BuildType { get; set; }

        public bool HideDirectoryStructure { get; set; } //混淆目录结构

        public bool Valid { get; set; } //是否是有效的查询（这个查询结果在初始化的时候设为false）（有效的定义：加载程序拿着这个查询结果就可以去跑加载流程了）
    }
}
