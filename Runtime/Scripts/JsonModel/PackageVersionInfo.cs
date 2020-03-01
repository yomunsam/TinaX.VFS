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
        public string mainifestFileHash;    //Package中mainifest文件本身的hash（16位小写MD5）
    }
}
