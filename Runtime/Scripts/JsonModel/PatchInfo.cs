using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// 补丁信息
    /// </summary>
    [Serializable]
    public class PatchInfo
    {
        /// <summary>
        /// 版本分支
        /// </summary>
        public string branch;
        /// <summary>
        /// 目标版本号（母包版本）
        /// </summary>
        public long targetVersionCode;

        /// <summary>
        /// 目标平台
        /// </summary>
        public XRuntimePlatform platform;

        public long patchCode;
        public bool extension;
        public string extensionGroupName;

    }
}
