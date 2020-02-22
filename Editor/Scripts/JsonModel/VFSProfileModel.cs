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

        #region 序列化存储用
        /// <summary>
        /// 外部不要直接操作它！！！！
        /// </summary>
        public ProfileRecord[] Profiles; //存储 序列化


        #endregion


        #region 作为对象时的运算用
        private Dictionary<string, ProfileRecord> _dict_profiles;
        private Dictionary<string, ProfileRecord> dict_profiles
        {
            get
            {
                if(_dict_profiles == null)
                {
                    if (Profiles == null) Profiles = new ProfileRecord[0];
                    _dict_profiles = new Dictionary<string, ProfileRecord>();
                    foreach (var item in Profiles)
                    {
                        if (!_dict_profiles.ContainsKey(item.ProfileName))
                            _dict_profiles.Add(item.ProfileName, item);
                    }
                }
                return _dict_profiles;
            }
        }

        public bool TryGetProfille(string name,out ProfileRecord profile)
        {
            return dict_profiles.TryGetValue(name,out profile);
        }

        public void AddProfileIfNotExists(ProfileRecord pr)
        {
            if (!dict_profiles.ContainsKey(pr.ProfileName))
            {
                dict_profiles.Add(pr.ProfileName,pr);
            }
        }

        public void ReadySave()
        {
            List<ProfileRecord> temp = new List<ProfileRecord>();
            foreach(var item in dict_profiles)
            {
                temp.Add(item.Value);
            }
            Profiles = temp.ToArray();
        }

        #endregion

    }
}
