using System.Collections.Generic;
namespace TinaXEditor.VFSKitInternal
{
    /// <summary>
    /// 保存构建时的Profile信息和其他构建信息
    /// </summary>
    public class EditorBuildInfo
    {
        public string[] main_package_local;
        public string[] main_package_remote;
        public string[] extension_local;
        public string[] extension_remote;

        public string[] total_main_package;
        public string[] total_extension; //这里不包含disable的

        public string[] disable_extension;

        public string build_profile_name;


        internal List<string> list_main_package_local = new List<string>();
        internal List<string> list_main_package_remote = new List<string>();
        internal List<string> list_extension_local = new List<string>();
        internal List<string> list_extension_remote = new List<string>();

        internal List<string> list_total_main_package = new List<string>();
        internal List<string> list_total_extension = new List<string>();

        internal List<string> list_disable_extension = new List<string>();

        internal void ReadySave()
        {
            main_package_local = list_main_package_local.ToArray();
            main_package_remote = list_main_package_remote.ToArray();
            extension_local = list_extension_local.ToArray();
            extension_remote = list_extension_remote.ToArray();

            total_main_package = list_total_main_package.ToArray();
            total_extension = list_total_extension.ToArray();

            disable_extension = list_disable_extension.ToArray();

        }

    }
}
