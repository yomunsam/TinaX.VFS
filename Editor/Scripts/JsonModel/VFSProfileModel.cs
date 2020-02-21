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

        #endregion

    }
}
