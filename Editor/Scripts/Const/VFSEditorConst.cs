using TinaXEditor.Const;
using TinaX.VFSKit.Const;

namespace TinaXEditor.VFSKit.Const
{
    public static class VFSEditorConst
    {
        public static string VFSProfileProjectSettingFileName = "VFSProfiles.json";

        /// <summary>
        /// 在工程目录下存放VFS打包文件的地方 （存放多个Source Packages的地方）
        /// </summary>
        public static string PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH => System.IO.Path.Combine(XEditorConst.TinaXProjectRootFolderPath, "VFS_Build");
        
        /// <summary>
        /// 调试文件服务器的文件根目录
        /// </summary>
        public static string PROJECT_VFS_FILES_SERVER_FOLDER_PATH => System.IO.Path.Combine(XEditorConst.TinaXProjectLibraryRootFolder, "VFS_FileServer");
        public static string PROJECT_VFS_FILES_FOLDER_MAIN => VFSConst.VFS_FOLDER_MAIN;
        /// <summary>
        /// 扩展包
        /// </summary>
        public static string PROJECT_VFS_FILES_FOLDER_EXTENSION => VFSConst.VFS_FOLDER_EXTENSION;
        /// <summary>
        /// 云端资源
        /// </summary>
        public const string PROJECT_VFS_FILE_FOLDER_REMOTE = VFSConst.VFS_FOLDER_REMOTE;

        /// <summary>
        /// 存放VFS每次打包的数据的文件
        /// </summary>
        public static string PROJECT_VFS_FILE_FOLDER_DATA = VFSConst.VFS_FOLDER_DATA; 

        public const string VFS_Version_Record_File_Name = "VFSVersion.json";
        public static string VFS_VERSION_RECORD_FILE_PATH => System.IO.Path.Combine(VFS_VERSION_ROOT_FOLDER_PATH, "Data", VFS_Version_Record_File_Name);
        /// <summary>
        /// 所有分支信息的根目录
        /// </summary>
        public static string VFS_VERSION_ROOT_FOLDER_PATH => System.IO.Path.Combine(XEditorConst.TinaXProjectRootFolderPath, "VFS_Version");
        public static string VFS_VERSION_RECORD_Data_FOLDER_PATH => System.IO.Path.Combine(VFS_VERSION_ROOT_FOLDER_PATH, "Data");
        public static string VFS_VERSION_RECORD_Binary_FOLDER_PATH => System.IO.Path.Combine(VFS_VERSION_ROOT_FOLDER_PATH, "Binary");

        public const string VFS_VERSION_AssetsBinary_Zip_Name = "assets.zip";
        public const string VFS_VERSION_AssetsBinary_REMOTE_Zip_Name = "assets_remote.zip";

        public const string VFS_EditorBuildInfo_FileName = "editor_build_info.json";
    }
}
