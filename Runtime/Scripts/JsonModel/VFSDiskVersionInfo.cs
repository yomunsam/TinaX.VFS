using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit.Version
{
    [Serializable]
    public class VFSDiskVersionInfo
    {
        //母包资源版本
        public long VFSPackageVersion;
        public string VFSPackageVersionName;

        //补丁版本
        public long VFSPatchVersion;
        public string VFSPatchVersionName;

        //扩展包的版本是独立的，不放在这里

    }
}
