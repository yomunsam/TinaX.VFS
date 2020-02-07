using System;
using System.Linq;
using System.Collections.Generic;
using TinaX.VFSKit;

namespace TinaX.VFSKitInternal.Utils
{
    public static class VFSUtil
    {

        /// <summary>
        /// 是否为子路径， 如果相等也返回false
        /// </summary>
        /// <param name="path1">路径1</param>
        /// <param name="path2">路径2</param>
        /// <param name="mutual">是否互相检测，如果为false，只检查path1是否为path2的子路径；如果为true，互相判断两者是否为对方的子路径</param>
        /// <returns></returns>
        public static bool IsSubpath(string path1, string path2, bool mutual = false)
        {
            string p1 = (path1.EndsWith("/") || path1.EndsWith("\\")) ? path1.Substring(0, path1.Length - 1) : path1;
            string p2 = (path2.EndsWith("/") || path2.EndsWith("\\")) ? path2.Substring(0, path2.Length - 1) : path2;

            if (p1 == p2) return false;

            if (mutual)
            {
                //相互判断
                return (p1.StartsWith(p2) || p2.StartsWith(p1));
            }
            else
            {
                //判断p1是否是p2的子路径
                return p2.StartsWith(p1);
            }

        }

        /// <summary>
        /// 是否为子路径或相同路径
        /// </summary>
        /// <param name="path1">路径1</param>
        /// <param name="path2">路径2</param>
        /// <param name="mutual">是否互相检测，如果为false，只检查path1是否为path2的子路径；如果为true，互相判断两者是否为对方的子路径</param>
        /// <returns></returns>
        public static bool IsSameOrSubPath(string path1, string path2, bool mutual = false)
        {
            string p1 = (path1.EndsWith("/") || path1.EndsWith("\\")) ? path1.Substring(0, path1.Length - 1) : path1;
            string p2 = (path2.EndsWith("/") || path2.EndsWith("\\")) ? path2.Substring(0, path2.Length - 1) : path2;

            if (p1 == p2) return true;

            if (mutual)
            {
                //相互判断
                return (p1.StartsWith(p2) || p2.StartsWith(p1));
            }
            else
            {
                //判断p1是否是p2的子路径
                return p2.StartsWith(p1);
            }
        }

        public static bool IsInResources(string path)
        {
            return path.Replace('\\', '/').ToLower().Contains("/resources/");
        }

        /// <summary>
        /// 配置规范化
        /// </summary>
        /// <param name="config"></param>
        public static void NormalizationConfig(ref VFSConfigModel config)
        {
            if (config == null) return;
            //全局配置

            //后缀名配置 补全缺失的“框架内部定义的必须存在的”配置项
            #region 全局 后缀名 内部配置项
            if (config.GlobalVFS_Ignore_ExtName == null)
                config.GlobalVFS_Ignore_ExtName = new string[0];
            List<string> ext_list = new List<string>();
            foreach (var item in TinaX.VFSKitInternal.InternalVFSConfig.GlobalIgnoreExtName)
            {
                if (!config.GlobalVFS_Ignore_ExtName.Contains(item))
                {
                    ext_list.Add(item);
                }
            }
            if (ext_list.Count > 0)
                ArrayUtil.Combine(ref config.GlobalVFS_Ignore_ExtName, ext_list.ToArray());
            #endregion

            //后缀名配置必须要以点开头，并小写
            #region 全局 后缀名 格式规范
            if (config.GlobalVFS_Ignore_ExtName != null && config.GlobalVFS_Ignore_ExtName.Length > 0)
            {
                for (var i = 0; i < config.GlobalVFS_Ignore_ExtName.Length; i++)
                {
                    if (!config.GlobalVFS_Ignore_ExtName[i].StartsWith("."))
                        config.GlobalVFS_Ignore_ExtName[i] = "." + config.GlobalVFS_Ignore_ExtName[i];

                    config.GlobalVFS_Ignore_ExtName[i] = config.GlobalVFS_Ignore_ExtName[i].ToLower();
                }
            }
            #endregion

            //后缀名配置 重复项
            #region 全局 后缀名 重复项
            if (config.GlobalVFS_Ignore_ExtName != null && config.GlobalVFS_Ignore_ExtName.Length > 0)
            {
                List<string> list = new List<string>(config.GlobalVFS_Ignore_ExtName).Distinct().ToList();
                if (list.Count != config.GlobalVFS_Ignore_ExtName.Length)
                    config.GlobalVFS_Ignore_ExtName = list.ToArray();
            }
            #endregion


            #region 全局 忽略 路径 item

            //补全内部设定
            if (config.GlobalVFS_Ignore_Path_Item == null)
                config.GlobalVFS_Ignore_Path_Item = new string[0];
            List<string> ignore_pathitem_list = new List<string>();
            foreach (var item in TinaX.VFSKitInternal.InternalVFSConfig.GlobalIgnorePathItem)
            {
                if (!config.GlobalVFS_Ignore_Path_Item.Contains(item))
                {
                    ignore_pathitem_list.Add(item);
                }
            }
            if (ignore_pathitem_list.Count > 0)
                ArrayUtil.Combine(ref config.GlobalVFS_Ignore_Path_Item, ignore_pathitem_list.ToArray());

            #endregion

        }
    }

}
