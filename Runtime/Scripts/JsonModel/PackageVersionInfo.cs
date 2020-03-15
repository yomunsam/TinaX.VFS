using System;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// Package 版本信息
    /// </summary>
    [Serializable]
    public class PackageVersionInfo
    {
        public long version;
        public string versionName;
        public string buildId;    //build info 中记录的id
        public string branch;
    }
}
