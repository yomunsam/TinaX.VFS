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

        public void Register(VFSExtensionGroup group)
        {
            lock (this)
            {
                if (!mGroups.Contains(group))
                    mGroups.Add(group);
                if (mDict_Groups.ContainsKey(group.GroupName.ToLower()))
                    mDict_Groups[group.GroupName.ToLower()] = group;
                else
                    mDict_Groups.Add(group.GroupName.ToLower(), group);
            }
        }

        public bool TryGetExtensionGroup(string groupName, out VFSExtensionGroup group)
        {
            return mDict_Groups.TryGetValue(groupName, out group);
        }

        public bool IsExists(string groupName)
        {
            return mDict_Groups.ContainsKey(groupName.ToLower());
        }

    }
}
