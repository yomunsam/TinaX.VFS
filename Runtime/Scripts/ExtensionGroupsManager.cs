using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.VFSKit;


namespace TinaX.VFSKitInternal
{
    internal class ExtensionGroupsManager
    {
        private VFSKit.VFSKit mVFS;
        public List<VFSExtensionGroup> mGroups = new List<VFSExtensionGroup>();
        public Dictionary<string, VFSExtensionGroup> mDict_Groups = new Dictionary<string, VFSExtensionGroup>();

        public ExtensionGroupsManager(VFSKit.VFSKit vfs)
        {
            mVFS = vfs;
        }

        public bool TryGetExtensionGroup(string groupName, out VFSExtensionGroup group)
        {
            return mDict_Groups.TryGetValue(groupName, out group);
        }

        public bool IsGroupEnabled(string groupName)
        {
            return mDict_Groups.ContainsKey(groupName);
        }

    }
}
