using System.IO;
using TinaX.VFS.Consts;

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
    }
}
