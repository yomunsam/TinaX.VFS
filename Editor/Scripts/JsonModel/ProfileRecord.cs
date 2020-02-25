using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;

namespace TinaXEditor.VFSKitInternal
{
    [Serializable]
    public class ProfileRecord
    {
        [Serializable]
        public struct S_GroupProfileRecord
        {
            public string GroupName;
            public E_GroupAssetsLocation Location;
            public bool DisableGroup;
        }

        [Serializable]
        public enum E_GroupAssetsLocation
        {
            Local = 0,
            Remote = 1,
        }

        public string ProfileName = string.Empty;

        //public bool DevelopMode = false;

        /// <summary>
        /// 资源存储位置
        /// </summary>
        public S_GroupProfileRecord[] GroupProfileRecords = { };


        public ProfileRecord DefaultByGrousp(List<VFSGroup> groups)
        {
            List<S_GroupProfileRecord> temp = new List<S_GroupProfileRecord>();
            foreach(var group in groups)
            {
                var t_gr = new S_GroupProfileRecord();
                t_gr.GroupName = group.GroupName;
                switch (group.HandleMode)
                {
                    case GroupHandleMode.LocalOnly:
                    case GroupHandleMode.LocalAndUpdatable:
                    case GroupHandleMode.LocalOrRemote:
                        t_gr.Location = E_GroupAssetsLocation.Local;
                        break;

                    case GroupHandleMode.RemoteOnly:
                        t_gr.Location = E_GroupAssetsLocation.Remote;
                        break;

                    default:
                        t_gr.Location = E_GroupAssetsLocation.Local;
                        break;
                }
                t_gr.DisableGroup = false;

            }
            GroupProfileRecords = temp.ToArray();
            return this;
        }

        public ProfileRecord SetProfileName(string name)
        {
            this.ProfileName = name;
            return this;
        }

        public bool IsDisabledGroup(string groupName)
        {
            string groupName_Lower = groupName.ToLower();
            foreach(var record in GroupProfileRecords)
            {
                if(groupName_Lower == record.GroupName.ToLower())
                {
                    return record.DisableGroup;
                }
            }
            return false;
        }

        public bool TryGetGroupLocation(string groupName,out E_GroupAssetsLocation location)
        {
            var grouplower = groupName.ToLower();
            foreach (var record in GroupProfileRecords)
            {
                if (grouplower == record.GroupName.ToLower())
                {
                    location = record.Location;
                    return true;
                }
            }
            location = default;
            return false;
        }

    }
}
