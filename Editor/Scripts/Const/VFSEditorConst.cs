using TinaXEditor.Const;
using TinaX.VFSKit.Const;

namespace TinaXEditor.VFSKit.Const
{
    public static class VFSEditorConst
    {
        public static string VFSProfileProjectSettingFileName = "VFSProfiles.json";

        /// <summary>
        /// 在工程目录下存放VFS打包文件的地方
        /// </summary>
        public static string PROJECT_VFS_FILES_ROOT_FOLDER_PATH => System.IO.Path.Combine(XEditorConst.TinaXProjectRootFolderPath, "VFS_Build");
        public static string PROJECT_VFS_FILES_FOLDER_MAIN => VFSConst.VFS_FOLDER_MAIN;
        /// <summary>
        /// 扩展包
        /// </summary>
        public static string PROJECT_VFS_FILES_FOLDER_EXTENSION => VFSConst.VFS_FOLDER_EXTENSION;
        /// <summary>
        /// 云端资源
        /// </summary>
        public static string PROJECT_VFS_FILE_FOLDER_REMOTE = "vfs_remote";
        public static string PROJECT_VFS_FILE_FOLDER_DATA = VFSConst.VFS_FOLDER_EXTENSION;

    }
}
