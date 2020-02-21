using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKitInternal
{
    [Serializable]
    public class ProfileRecord
    {
        [Serializable]
        public struct S_GroupAssetsLocation
        {
            public string GroupName;
            public E_GroupAssetsLocation Location;
        }

        [Serializable]
        public enum E_GroupAssetsLocation
        {
            Local = 0,
            Server = 1,
        }

        public string ProfileName = string.Empty;

        /// <summary>
        /// 资源存储位置
        /// </summary>
        public S_GroupAssetsLocation[] GroupAssetsLocations = { };
    }
}
