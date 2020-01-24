using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    public enum VFSErrorCode
    {
        /// <summary>
        /// Load VFS Config Failed | 加载VFS配置 失败
        /// </summary>
        LoadConfigFailed            = 1,


        /// <summary>
        /// 资源组规则冲突
        /// </summary>
        ConfigureGroupsConflict     = 200,


    }
}
