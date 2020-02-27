using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit.Versions
{
    [Serializable]
    public class VersionBranch
    {
        [Serializable]
        public enum BranchType
        {
            MainPackage     = 0,
            ExtensionGroup  = 1,
        }

        public string BranchName;

        /// <summary>
        /// branch type | 分支类型
        /// </summary>
        public BranchType BType;

        public string Desc;

        /// <summary>
        /// if branch type is extension group.
        /// </summary>
        public string ExtensionGroupName;

        public VersionRecord[] VersionRecords;

    }

    public struct VersionRecord
    {
        public long versionCode;
        public string versionName;
        public string desc;

        public string dataPath;
        public string filePackagePath;
    }
}
