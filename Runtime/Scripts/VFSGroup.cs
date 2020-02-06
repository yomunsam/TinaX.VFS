using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinaX.VFSKitInternal.Utils;


namespace TinaX.VFSKit
{
    public class VFSGroup
    {
        public string GroupName { get; set; }
        public List<string> FolderPaths { get; private set; } = new List<string>();

        private VFSGroupOption mOption;

        public VFSGroup()
        {

        }

        public VFSGroup(VFSGroupOption option)
        {
            mOption = option;
            GroupName = option.GroupName;
            FolderPaths.AddRange(option.FolderPaths);

        }

        /// <summary>
        /// 检查组内的文件夹冲突，并将存在冲突的内容返回，如果没有冲突则返回值的Count = 0
        /// </summary>
        /// <returns></returns>
        public List<string> CheckFolderConflict()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < FolderPaths.Count; i++)
            {
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (VFSUtil.IsSameOrSubPath(FolderPaths[i], FolderPaths[j], true))
                        {
                            if (!result.Contains(FolderPaths[i]))
                            {
                                result.Add(FolderPaths[i]);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 检查组内的文件夹是否冲突，如果冲突，返回true，冲突的文件夹路径会放在out参数里
        /// </summary>
        /// <param name="simplify">简化检测，如果有发现冲突直接返回false结果，而不遍历所有数据进行判断</param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public bool CheckFolderConflict(out List<string> paths, bool simplify = false)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < FolderPaths.Count; i++)
            {
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (VFSUtil.IsSameOrSubPath(FolderPaths[i], FolderPaths[j], true))
                        {
                            if (!result.Contains(FolderPaths[i]))
                            {
                                result.Add(FolderPaths[i]);
                            }

                            if (simplify)
                            {
                                paths = result;
                                return true;
                            }

                        }
                    }
                }
            }
            paths = result;
            return (result.Count > 0);
        }

        /// <summary>
        /// 检查给定的文件夹路径是否与组内的文件夹配置冲突（相同或者互为子路径），如果冲突，返回true
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool CheckFolderConflict(string folderPath)
        {
            foreach(var path in FolderPaths)
            {
                if (VFSUtil.IsSameOrSubPath(folderPath, path, true))
                {
                    return true;
                }
            }
            return false;
        }

    }
}


