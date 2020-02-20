using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKitInternal
{
    [Serializable]
    public class VFSProfileModel
    {
        [Serializable]
        public class ProfileItem 
        {
            public string ProfileName = string.Empty;

            /// <summary>
            /// 资源存储位置
            /// </summary>
            public S_GroupAssetsLocation[] GroupAssetsLocations;

        }

        [Serializable]
        public struct S_GroupAssetsLocation
        {
            public string GroupName;
            public E_GroupAssetsLocation Location;
        }

        [Serializable]
        public enum E_GroupAssetsLocation
        {
            Local        = 0,
            Server       = 1,
        }


        #region 序列化存储用
        public ProfileItem[] Profiles; //存储 序列化
        #endregion


        #region 作为对象时的运算用
        private Dictionary<string, ProfileItem> _dict_profiles;
        private Dictionary<string, ProfileItem> dict_profiles
        {
            get
            {
                if(_dict_profiles == null)
                {
                    _dict_profiles = new Dictionary<string, ProfileItem>();
                    foreach (var item in Profiles)
                    {
                        if (!_dict_profiles.ContainsKey(item.ProfileName))
                            _dict_profiles.Add(item.ProfileName, item);
                    }
                }
                return _dict_profiles;
            }
        }

        public bool TryGetProfille(string name,out ProfileItem profile)
        {
            return dict_profiles.TryGetValue(name,out profile);
        }

        #endregion

    }
}
