using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.VFSKit;

namespace TinaXEditor.VFSKit
{
    /// <summary>
    /// 资源状态查询结果
    /// </summary>
    public struct AssetsStatusQueryResult
    {
        public string AssetPath { get; set; }

        /// <summary>
        /// 是否被VFS管理
        /// </summary>
        public bool ManagedByVFS
        {
            get
            {
                if (!this.InWhitelist) return false; //压根不在组的管理白名单中，返回false
                if (this.InWhitelist)
                {
                    if (this.IgnoreByGlobalRule) return false; //被全局规则所忽略
                    if (this.IgnoreByGroupRule) return false; //被组内规则忽略。
                }
                if (GroupName.IsNullOrEmpty()) return false;
                
                return true;
            }
        }

        /// <summary>
        /// 在某组的白名单中
        /// </summary>
        public bool InWhitelist { get; set; }

        /// <summary>
        /// 所在组名（在InWhitelist 为 true 时有效）
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 被所在组的规则忽略
        /// </summary>
        public bool IgnoreByGroupRule
        {
            get
            {
                return (IgnoreByGroup_IgnoreSubPath_List || IgnoreByGroup_IgnoreExtName_List);
            }
        }

        /// <summary>
        /// 被Group中的“IgnoreSubPath”规则所忽略
        /// </summary>
        public bool IgnoreByGroup_IgnoreSubPath_List { get; set; }

        /// <summary>
        /// 被Group在的“IgnoreExtName”规则所忽略
        /// </summary>
        public bool IgnoreByGroup_IgnoreExtName_List { get; set; }

        /// <summary>
        /// 被全局规则所忽略
        /// </summary>
        public bool IgnoreByGlobalRule
        {
            get
            {
                if (IgnoreByGlobal_IgnoreExtName_List || IgnoreByGlobal_IgnorePathItem_List)
                    return true;
                else
                    return false;
            }
        }

        
        public bool IgnoreByGlobal_IgnoreExtName_List { get; set; }

        public bool IgnoreByGlobal_IgnorePathItem_List { get; set; }


        public string AssetBundleFileName { get; set; }
        public FolderBuildType BuildType { get; set; }
        public FolderBuildDevelopType DevType { get; set; }

    }
}
