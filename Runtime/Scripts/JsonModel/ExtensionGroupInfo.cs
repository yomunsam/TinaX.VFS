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
        /// <summary>
        /// Assetbundle file extension name | ab文件后缀名
        /// </summary>
        public string AssetBundleExtension;
    }
}
