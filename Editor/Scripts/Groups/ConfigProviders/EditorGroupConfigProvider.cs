using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Groups.ConfigProviders;
using TinaXEditor.VFS.Groups.Utils;
using TinaXEditor.VFS.Scripts.ConfigProviders;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.VFS.Groups.ConfigProviders
{
    public class EditorGroupConfigProvider : GroupConfigProvider, IEditorConfigProvider<GroupConfigTpl>
    {
        protected readonly GroupConfigTpl m_EditorConfig;

        public EditorGroupConfigProvider(GroupConfigTpl configTpl) : base(configTpl)
        {
            //这个提供者继承了Runtime下的提供者，它需要提供Runtime格式的配置和Editor标准的配置，
            //于是，奇妙的深拷贝大法
            var json = EditorJsonUtility.ToJson(configTpl);
            m_EditorConfig = JsonUtility.FromJson<GroupConfigTpl>(json);
        }

        public GroupConfigTpl EditorConfiguration => m_EditorConfig;

        public override void Standardize()
        {
            EditorGroupStandardizationUtil.StandardizeGroup(m_EditorConfig);

            base.Standardize(); //这个里面做了Runtime的标准化
        }
    }
}
