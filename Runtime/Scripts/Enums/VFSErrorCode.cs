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
        /// The path being loaded is invalid | 被加载的path是无效的
        /// </summary>
        ValidLoadPath               = 3,

        InitWebVFSError             = 4,

        DownloadNetworkError        = 5,
        ExtensionPackageVersionConflict = 6,    //扩展包版本冲突
        VersionInfoInValid          = 7, //版本信息无效

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
