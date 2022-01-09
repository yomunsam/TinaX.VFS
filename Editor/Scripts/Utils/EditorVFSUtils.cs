using System.IO;
using TinaX.VFS.Consts;
using TinaXEditor.VFS.Const;

namespace TinaXEditor.VFS.Packages.io.nekonya.tinax.vfs.Editor.Scripts.Utils
{
    public static class EditorVFSUtils
    {
        /// <summary>
        /// 获取项目工程中（Editor），VFS构建AssetBundle的默认输出位置
        /// </summary>
        /// <returns></returns>
        public static string GetProjectAssetBundleOutputPath()
            => Path.Combine(Directory.GetCurrentDirectory(), VFSConsts.ProjectVFSArchiveFolder, "Build");

        /// <summary>
        /// 获取项目工程中（Editor），VFS构建AssetBundle的默认输出位置
        /// </summary>
        /// <returns></returns>
        public static string GetProjectAssetBundleOutputPath(string platformName)
            => Path.Combine(Directory.GetCurrentDirectory(), VFSConsts.ProjectVFSArchiveFolder, "Build", platformName);

        public static string GetAssetsHashFilePath()
            => Path.Combine(Directory.GetCurrentDirectory(), VFSConsts.ProjectVFSArchiveFolder, "Build", "Data", VFSEditorConsts.AssetsHashFileName);

    }
}
