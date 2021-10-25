using TinaX.VFS.BuildRules;
using TinaX.VFS.Packages;
using TinaXEditor.VFS.Groups;

namespace TinaXEditor.VFS.Querier
{
    /// <summary>
    /// 编辑器下的资产查询结果
    /// </summary>
    public struct EditorAssetQueryResult
    {
        /// <summary>
        /// 编辑器下的AssetPath应该是和工程"Assets/"目录下的内容完全对应的
        /// </summary>
        public string AssetPath { get; set; }
        public string AssetPathLower { get; set; }
        public string AssetExtension { get; set; } //资产的文件后缀名（比如说检查后缀名黑名单的时候会用到）

        /// <summary>
        /// 虚拟资产路径
        /// 虚拟资产路径是Runtime下加载Asset的加载路径，由于变体规则等问题，虚拟资产路径与编辑器下的AssetPath可能会不一致
        /// </summary>
        public string VirtualAssetPath { get; set; }

        /// <summary>
        /// 虚拟资产路径 小写
        /// </summary>
        public string VirtualAssetPathLower { get; set; }

        public bool ManagedByMainPack { get; set; }//是否被主包管理的资产
        public VFSPackage ManagedPackage { get; set; } //该资产被哪个包管理
        public EditorVFSGroup ManagedGroup { get; set; } //该资产被哪个组管理
        public string AssetBundleName { get; set; } //其他程序通过这个地址实际加载AssetBundle
        public string OriginalAssetBundleName { get; set; } //原始AssetBundle名称，指通过打包规则推断出的原始名称，比如说后续某个流程打算混淆AssetBundleName的话，改上面哪个，这儿还应该是原始名称不动

        public string FileNameInAssetBundle { get; set; } //这个资产在AssetBundle包内的文件名（实际加载使用） （Unity的Scriptable build pipline的文档的案例标题中把这个叫做File Name，但它的代码里又叫addressableNames）
        public string OriginalFileNameInAssetBundle { get; set; } //原始FileNameInAssetBundle值

        public bool IsVariant { get; set; } //原始AssetPath是一个变体资产
        public string VariantName { get; set; } //资产变体名（如果不是变体则留空，或者也可以设置为VFS全局配置里的默认变体名）
        public string VariantSourceAssetPath { get; set; } //根据变体还原推导出的AssetPath，通常情况下外部应该直接用VirtualAssetPath

        public FolderBuildType BuildType { get; set; }

        public bool HideDirectoryStructure { get; set; } //混淆目录结构

        public bool Valid { get; set; } //是否是有效的查询（这个查询结果在初始化的时候设为false）（有效的定义：加载程序拿着这个查询结果就可以去跑加载流程了）
    }
}
