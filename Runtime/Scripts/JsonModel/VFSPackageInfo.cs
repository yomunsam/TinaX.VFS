using System;

namespace TinaX.VFSKit
{
    [Serializable]
    public class VFSPackageInfo
    {
        public XRuntimePlatform packagePlatform;
        public long versionCode;
        public string versionName;
        public string[] VFSGroupNames; //不包括可扩展组
    }
}
