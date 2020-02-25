using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinaX.VFSKitInternal.Utils;
using TinaX.IO;


namespace TinaX.VFSKit
{
    public class VFSGroup
    {
        public string GroupName { get; set; }


        public GroupHandleMode HandleMode => mOption.GroupAssetsHandleMode;

        /// <summary>
        /// 扩展的组
        /// </summary>
        public bool ExtensionGroup => mOption.ExtensionGroup;

        /// <summary>
        /// 储存FolderPath， 格式：Assets/xx/xxx/ ，必须以斜线“/”结尾。
        /// </summary>
        public List<string> FolderPaths { get; private set; } = new List<string>();
        public List<string> FolderPathsLower { get; private set; } = new List<string>(); //同样必须以斜线结尾

        /// <summary>
        /// Group中的直接Assets名单，格式: Assets/xxx/xx.xxx
        /// </summary>
        public List<string> AssetPaths { get; private set; } = new List<string>();
        public List<string> AssetPathsLower { get; private set; } = new List<string>();

        /// <summary>
        /// 当前组的忽略子目录，格式Assets/xxx/xxx/ 必须以斜线“/”结束
        /// </summary>
        public List<string> IgnoreSubpath { get; private set; } = new List<string>();
        public List<string> IgnoreSubpathLower { get; private set; } = new List<string>();

        /// <summary>
        /// 忽略后缀名，小写，开头加点
        /// </summary>
        public List<string> IgnoreExtensionLower { get; private set; } = new List<string>();

        public List<FolderBuildRule> SpecialFolderBuildRules { get; private set; } = new List<FolderBuildRule>();
        public List<FolderBuildRule> SpecialFolderBuildRulesLower { get; private set; } = new List<FolderBuildRule>();

        private VFSGroupOption mOption;

        public VFSGroup()
        {

        }

        public VFSGroup(VFSGroupOption option)
        {
            this.SetOptions(option);
        }

        public void SetOptions(VFSGroupOption option)
        {
            mOption = option;
            GroupName = option.GroupName;
            foreach(var path in option.FolderPaths)
            {
                if (!path.EndsWith("/"))
                {
                    string _path = path+"/";
                    FolderPaths.Add(_path);
                    FolderPathsLower.Add(_path.ToLower());
                }
                else
                {
                    FolderPaths.Add(path);
                    FolderPathsLower.Add(path.ToLower());
                }
            }

            foreach(var path in option.AssetPaths)
            {
                AssetPaths.Add(path);
                AssetPathsLower.Add(path.ToLower());
            }

            //忽略子目录，子目录必须是FolderPaths的子目录，这里初始化的时候过滤一下无效的配置，节省后面的运算
            foreach(var path in option.IgnoreSubPath)
            {
                string _path = (path.EndsWith("/")) ? path : path + "/";
                string path_lower = _path.ToLower();
                foreach(var folder in FolderPathsLower)
                {
                    if(VFSUtil.IsSubpath(path_lower, folder, false))
                    {
                        IgnoreSubpath.Add(_path);
                        IgnoreSubpathLower.Add(path_lower);
                        break;
                    }
                }
            }
        
            //忽略后缀名
            foreach(var ext in option.IngnoreExtName)
            {
                IgnoreExtensionLower.Add(ext.StartsWith(".") ? ext.ToLower() : "." + ext.ToLower());
            }

            //特殊打包规则
            foreach(var rule in option.FolderSpecialBuildRules)
            {
                bool flag = true;
                if (rule.DevType == FolderBuildDevelopType.normal && rule.BuildType == FolderBuildType.normal)
                    flag = false;//这是条没必要的规则
                if (rule.FolderPath.IsNullOrEmpty() || rule.FolderPath.IsNullOrWhiteSpace())
                    flag = false;

                string _folder_path = (rule.FolderPath.EndsWith("/")) ? rule.FolderPath : rule.FolderPath + "/";
                string _folder_lower = _folder_path.ToLower();

                if (!IsSubfolderOfFolderList(_folder_path))
                    flag = false;

                if (flag)
                {
                    var _rule = rule;
                    _rule.FolderPath = _folder_path;
                    var lower_rule = rule;
                    lower_rule.FolderPath = _folder_lower;
                    SpecialFolderBuildRules.Add(_rule);
                    SpecialFolderBuildRulesLower.Add(lower_rule);

                }
            }
        
        }

        /// <summary>
        /// 传入一个Asset的Path,检查是否在当前Group的规则内
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool IsAssetPathMatch(string assetPath)
        {
            string path_lower = assetPath.ToLower();

            //后缀名过滤
            foreach(var ext in IgnoreExtensionLower)
            {
                if (path_lower.EndsWith(ext))
                {
                    return false;
                }
            }

            bool whitelist = false;

            //在Asset白名单里？
            if (AssetPathsLower.Contains(path_lower))
                whitelist = true;

            //在folder里？
            if (!whitelist)
            {
                foreach(var path in FolderPathsLower)
                {
                    if (_IsAssetPathMatchFolder(ref path_lower, path))
                    {
                        whitelist = true;
                        break;
                    }
                }
            }

            if (!whitelist)
                return false;

            //检查是否在Group的黑名单里
            foreach(var ignorePath in IgnoreSubpathLower)
            {
                if(_IsAssetPathMatchFolder(ref path_lower, ignorePath))
                {
                    //命中忽略名单
                    return false;
                }
            }

            return true;
        }



        //给定的资源path是否包含在Folder中
        private bool _IsAssetPathMatchFolder(ref string assetPath, string folderPath)
        {
            if (assetPath.Length < folderPath.Length) return false;
            return assetPath.StartsWith(folderPath);
        }

        /// <summary>
        /// 【返回值不含AssetBundle后缀！】传入一个 Asset的Path，根据“特殊文件打包规则”推测它的AssetBundle名
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public string GetAssetBundleNameOfAsset(string assetPath,out FolderBuildType buildType, out FolderBuildDevelopType devType)
        {
            string assetPath_lower = assetPath.ToLower();
            //是否命中特殊目录
            foreach(var rule in SpecialFolderBuildRulesLower)
            {
                if(_IsAssetPathMatchFolder(ref assetPath_lower, rule.FolderPath))
                {
                    buildType = rule.BuildType;
                    devType = rule.DevType;
                    switch (rule.BuildType)
                    {
                        case FolderBuildType.normal:
                            return assetPath_lower;
                        case FolderBuildType.sub_dir:
                            string subs_path = assetPath_lower.Substring(rule.FolderPath.Length, assetPath_lower.Length - rule.FolderPath.Length);
                            int sub_index = subs_path.IndexOf('/');
                            if(sub_index == -1)
                            {
                                //路径里没有子目录。
                                return assetPath_lower;
                            }
                            else
                            {
                                //有子目录，取到子目录那一层
                                return rule.FolderPath + subs_path.Substring(0, sub_index);
                            }
                        case FolderBuildType.whole:
                            return rule.FolderPath;
                    }

                }
            }

            //没有命中
            buildType = FolderBuildType.normal;
            devType = FolderBuildDevelopType.normal;
            return assetPath_lower;
        }

        /// <summary>
        /// 【返回值不含AssetBundle后缀！】传入一个 Asset的Path，根据“特殊文件打包规则”推测它的AssetBundle名
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public string GetAssetBundleNameOfAsset(string assetPath)
        {
            return this.GetAssetBundleNameOfAsset(assetPath, out _, out _);
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

        /// <summary>
        /// 是否是FolderPathsLower的子目录
        /// </summary>
        /// <returns></returns>
        private bool IsSubfolderOfFolderList(string path)
        {
            string path_lower = path.ToLower();
            foreach(var folder in FolderPathsLower)
            {
                if (path_lower.StartsWith(folder))
                    return true;
            }
            return false;
        }
    }
}


