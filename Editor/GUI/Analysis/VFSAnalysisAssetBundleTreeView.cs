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
    public class VFSAnalysisAssetBundleTreeView : TreeView
    {
        public VFSAnalysisAssetBundleTreeView(TreeViewState state) : base(state)
        {
            Reload();
        }
        internal Dictionary<int, VFSBundle> Dict_Bundle_id = new Dictionary<int, VFSBundle>();
        internal Dictionary<int, VFSAsset> Dict_Assets_id = new Dictionary<int, VFSAsset>();

        protected override TreeViewItem BuildRoot()
        {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we create a fixed set of items. In a real world example,
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique. The root item is required to 
            // have a depth of -1, and the rest of the items increment from that.
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            //var allItems = new List<TreeViewItem>
            //{
            //    new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
            //    new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
            //    new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
            //    new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
            //    new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
            //    new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
            //    new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
            //    new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
            //    new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
            //};
            Dict_Bundle_id.Clear();
            Dict_Assets_id.Clear();
            var allItems = new List<TreeViewItem>();
            var root_item = new TreeViewItem { id = 1, depth = 0, displayName = "AssetBundle Assets" };
            allItems.Add(root_item);
            int counter = 1;
            var vfs = XCore.GetMainInstance().GetService<IVFSInternal>();
            if (vfs.LoadFromAssetbundle())
            {
                foreach(var bundle in vfs.GetAllBundle())
                {
                    counter++;
                    allItems.Add(new TreeViewItem {id = counter, depth = 1, displayName = bundle.AssetBundleName });
                    Dict_Bundle_id.Add(counter, bundle);
                    if (bundle.Assets != null && bundle.Assets.Count > 0)
                    {
                        foreach(var asset in bundle.Assets)
                        {
                            counter++;
                            allItems.Add(new TreeViewItem { id = counter, depth = 2, displayName = asset.AssetPathLower });
                            Dict_Assets_id.Add(counter, asset);
                        }
                    }
                }
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }
    }
}
