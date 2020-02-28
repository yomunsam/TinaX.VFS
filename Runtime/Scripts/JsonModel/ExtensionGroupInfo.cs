using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    [Serializable]
    public class ExtensionGroupInfo
    {
        public XRuntimePlatform Platform;
        public string GroupName;
        public long MainPackageVersionLimit;
    }
}
