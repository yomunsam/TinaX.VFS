using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Packages.ConfigProviders;
using TinaXEditor.VFS.Scripts.ConfigProviders;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFS.Packages.ConfigProviders
{
    public class EditorMainPackageConfigProvider : MainPackageConfigProvider, IEditorConfigProvider<MainPackageConfigTpl>
    {
        protected readonly MainPackageConfigTpl m_EditorConfig;

        public EditorMainPackageConfigProvider(MainPackageConfigTpl configTpl) : base(configTpl)
        {
            //奇妙深拷贝大法
            var json_str = EditorJsonUtility.ToJson(configTpl);
            m_EditorConfig = JsonUtility.FromJson<MainPackageConfigTpl>(json_str);
        }

        public MainPackageConfigTpl EditorConfiguration => m_EditorConfig;
    }
}
