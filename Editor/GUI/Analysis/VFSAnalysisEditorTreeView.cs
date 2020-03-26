using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using TinaX;
using TinaX.VFSKit;
using TinaX.VFSKitInternal;

namespace TinaXEditor.VFSKitInternal
{
    public class VFSAnalysisEditorTreeView : TreeView
    {
        public VFSAnalysisEditorTreeView(TreeViewState state) : base(state)
        {
            Reload();
        }
        internal Dictionary<int, EditorAsset> Dict_Assets_id = new Dictionary<int, EditorAsset>();

        protected override TreeViewItem BuildRoot()
        {
            
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            
            Dict_Assets_id.Clear();
            var allItems = new List<TreeViewItem>();
            var root_item = new TreeViewItem { id = 1, depth = 0, displayName = "Editor Simulation Assets" };
            allItems.Add(root_item);
            int counter = 1;
            var vfs = XCore.GetMainInstance().GetService<IVFSInternal>();
            if (!vfs.LoadFromAssetbundle())
            {
                foreach(var asset in vfs.GetAllEditorAsset())
                {
                    counter++;
                    allItems.Add(new TreeViewItem {id = counter, depth = 1, displayName = asset.AssetPathLower });
                    Dict_Assets_id.Add(counter, asset);
                }
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }
    }
}
