using System.Collections.Generic;
using System.Linq;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Groups;
using TinaX.VFS.Scripts.Structs;
using TinaXEditor.VFS.AssetBuilder.Structs;
using TinaXEditor.VFS.Scripts.ConfigProviders;
using UnityEditor;

namespace TinaXEditor.VFS.Groups
{
    public class EditorVFSGroup : VFSGroup
    {
        private readonly IEditorConfigProvider<GroupConfigTpl> m_EditorConfigProvider;

        public EditorVFSGroup(IEditorConfigProvider<GroupConfigTpl> configProvider) : base(configProvider)
        {
            this.m_EditorConfigProvider = configProvider;
        }

        /// <summary>
        /// 获取所有被我们组管理的Asset们
        /// </summary>
        /// <param name="groupAssets"></param>
        public virtual void GetAllManagedAssets(out List<AssetPathAndGuid> groupAssets)
        {
            groupAssets = new List<AssetPathAndGuid>();

            var editorConfig = m_EditorConfigProvider.EditorConfiguration;

            //AssetPaths
            foreach(var assetPath in editorConfig.AssetPaths)
            {
                if (string.IsNullOrEmpty(assetPath))
                    continue;
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
                if(!guid.Empty())
                {
                    groupAssets.Add(new AssetPathAndGuid(assetPath, guid));
                }
            }

            //FolderPaths整理
            var guids = AssetDatabase.FindAssets("", editorConfig.FolderPaths.ToArray());
            foreach(var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (groupAssets.Any(ga => ga.AssetPath == assetPath)) //已存在
                    continue;
                string assetPathLower = assetPath.ToLower();
                //管理检查
                if (this.IsMatchedAssetPathLower(assetPath)) 
                {
                    groupAssets.Add(new AssetPathAndGuid(assetPath, assetPathLower, guid));
                }
            }

        }

        //public virtual void GetAssetBundleInfo(AssetPathAndGuid assetPath, out AssetAndBundleInfo bundleInfo)
        //{
            

        //}

        //public virtual bool TryGetVariants(ref VFSAssetPath vfsAssetPath)
        //{

        //}

    }
}
