using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    public enum VFSErrorCode
    {
        Unknow                      = 0,
        /// <summary>
        /// Load VFS Config Failed | 加载VFS配置 失败
        /// </summary>
        LoadConfigFailed            = 1,

        FileNotFound                = 2,

        /// <summary>
        /// 资源组规则冲突
        /// </summary>
        ConfigureGroupsConflict     = 200,

        /// <summary>
        /// Same group name in config | 存在相同的配置名
        /// </summary>
        SameGroupName               = 201,

        /// <summary>
        /// No groups are configured | 没有任何Group在配置中
        /// </summary>
        NoneGroup                   = 202,

    }
}
