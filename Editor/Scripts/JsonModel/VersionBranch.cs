using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;

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

        public XRuntimePlatform Platform;

        public string Desc;

        /// <summary>
        /// if branch type is extension group.
        /// </summary>
        public string ExtensionGroupName;

        public VersionRecord[] VersionRecords;


        //==========================================================================================


        private List<VersionRecord> _list_records;

        private List<VersionRecord> mList_records
        {
            get
            {
                if(_list_records == null)
                {
                    _list_records = new List<VersionRecord>();
                    if (VersionRecords != null) _list_records.AddRange(VersionRecords);

                    SortRecordsList(ref _list_records);
                }
                return _list_records;
            }
        }

        public List<VersionRecord> VersionRecords_ReadWrite => mList_records;

        public VersionRecord? GetMaxVersion()
        {
            if (mList_records.Count > 0)
            {
                return mList_records[mList_records.Count - 1];
            }
            else
            {
                return null;
            }
        }

        public void ReadySave()
        {
            VersionRecords = mList_records.ToArray();
        }

        private void SortRecordsList(ref List<VersionRecord> list)
        {
            list.Sort((x, y) => x.versionCode.CompareTo(y.versionCode));
        }

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
