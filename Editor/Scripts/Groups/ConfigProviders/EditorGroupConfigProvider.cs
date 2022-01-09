using TinaX.VFS.ConfigAssets.Configurations;
using TinaX.VFS.Groups.ConfigProviders;
using TinaX.VFS.SerializableModels.Configurations;
using TinaXEditor.VFS.Groups.Utils;
using TinaXEditor.VFS.Scripts.ConfigProviders;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace TinaXEditor.VFS.Groups.ConfigProviders
{
    public class EditorGroupConfigProvider : GroupConfigProvider, IEditorConfigProvider<GroupConfig, GroupConfigModel>
    {
        protected readonly GroupConfig m_EditorConfig;

        public EditorGroupConfigProvider(GroupConfig config) : base(JsonUtility.FromJson<GroupConfigModel>(JsonUtility.ToJson(config)))
        {
            m_EditorConfig = config;
        }

        public EditorGroupConfigProvider(GroupConfig config, GroupConfigModel configModel) : base(configModel)
        {
            m_EditorConfig = config;
        }

        public GroupConfig EditorConfiguration => m_EditorConfig;


        public override void Standardize()
        {
            EditorGroupStandardizationUtil.StandardizeGroup(m_EditorConfig);

            base.Standardize(); //这个里面做了Runtime的标准化
        }
    }
}
