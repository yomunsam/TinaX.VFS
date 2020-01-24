using System;

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

    }

}
