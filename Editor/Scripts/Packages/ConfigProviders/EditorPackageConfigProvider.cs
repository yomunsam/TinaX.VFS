using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages.ConfigProviders;
using TinaXEditor.VFS.Scripts.ConfigProviders;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFS.Packages.ConfigProviders
{
    public class EditorPackageConfigProvider : PackageConfigProvider, IEditorConfigProvider<PackageConfigTpl>
    {
        protected readonly PackageConfigTpl m_EditorConfig;

        public EditorPackageConfigProvider(PackageConfigTpl configTpl) : base(configTpl)
        {
            //奇妙深拷贝大法
            var json_str = EditorJsonUtility.ToJson(configTpl);
            m_EditorConfig = JsonUtility.FromJson<PackageConfigTpl>(json_str);
        }

        public PackageConfigTpl EditorConfiguration => m_EditorConfig;
    }
}
