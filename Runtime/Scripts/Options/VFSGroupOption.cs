using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TinaX.VFSKitInternal.Utils;


namespace TinaX.VFSKit
{
    [Serializable]
    public class VFSGroupOption
    {
        internal static VFSGroupOption New() => new VFSGroupOption();
        internal static VFSGroupOption New(string groupName) => new VFSGroupOption() { GroupName = groupName };

        public string GroupName = "common";

        [Header("Asset folders in this group | 当前资源组下包含的资源目录.")]
        public string[] FolderPaths = { };

        [Header("Asset file in this group | 当前资源组下的资源文件.")]
        public string[] AssetPaths = { };

        [Header("Handle Mode")]
        public GroupHandleMode GroupAssetsHandleMode = GroupHandleMode.LocalAndUpdatable;

        [Header("Ignore subpath in this group's FolderPaths whitelist. | 在当前资源组下的 资源目录 配置中的 忽略子路径")]
        public string[] IgnoreSubPath = { };

        [Header("Ignore extend name. | 忽略的扩展名")]
        public string[] IngnoreExtName = { };

        /// <summary>
        /// 检查文件夹冲突，并将存在冲突的内容返回，如果没有冲突则返回值的Count = 0
        /// </summary>
        /// <returns></returns>
        public List<string> CheckFolderConflict()
        {
            List<string> result = new List<string>();
            for(int i = 0; i < FolderPaths.Length; i++)
            {
                if(i > 0)
                {
                    for(int j = 0; j < i; j++)
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
        /// 检查文件夹是否冲突，如果冲突，返回true，冲突的文件夹路径会放在out参数里
        /// </summary>
        /// <param name="simplify">简化检测，如果有发现冲突直接返回false结果，而不遍历所有数据进行判断</param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public bool CheckFolderConflict(out List<string> paths, bool simplify = false)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < FolderPaths.Length; i++)
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



    }
}

