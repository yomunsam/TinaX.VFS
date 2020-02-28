using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TinaX.VFSKit;
using TinaX;
using TinaX.IO;
using TinaX.VFSKit.Const;
using TinaX.VFSKitInternal.Utils;
using TinaXEditor.Const;
using TinaXEditor.VFSKit.Const;
using TinaXEditor.VFSKitInternal.I18N;
using TinaXEditor.VFSKitInternal;
using TinaXEditor.VFSKit.Versions;
using TinaX.VFSKit.Versions;

namespace TinaXEditor.VFSKit
{
    [InitializeOnLoad]
    public static class VFSManagerEditor
    {
        static VFSProfileModel VFSProfileEditor;

        static List<VFSGroup> Groups = new List<VFSGroup>();
        /// <summary>
        /// 整个VFS中所有Group整合的FolderPaths, 以斜线“/”结束。
        /// </summary>
        static List<string> FolderPaths = new List<string>();

        /// <summary>
        /// 整个VFS中所有Group整合的AssetPaths
        /// </summary>
        static List<string> AssetPaths = new List<string>();

        private static VFSVersionsManagerEditor _versionMgr;

        public static VFSVersionsManagerEditor VersionManager
        {
            get
            {
                if (_versionMgr == null) _versionMgr = new VFSVersionsManagerEditor();
                return _versionMgr;
            }
        }

        static VFSManagerEditor()
        {
            RefreshManager();
        }

        private static VFSConfigModel mConfig;

        public static VFSConfigModel VFSConfig => mConfig;

        public static System.DateTime LastRefreshManagerTime { get; private set; } = System.DateTime.UtcNow;

        public static void RefreshManager(bool Normalization = false)
        {
            mConfig = XConfig.GetConfig<VFSConfigModel>(VFSConst.ConfigFilePath_Resources);
            if (mConfig == null) return;
            if (Normalization)
            {
                VFSUtil.NormalizationConfig(ref mConfig);
            }

            if(!VFSUtil.CheckConfiguration(ref mConfig, out var errorCode, out var folderError))
            {
                string log_str= string.Empty;
                //配置文件校验未通过
                switch (errorCode)
                {
                    case VFSErrorCode.ConfigureGroupsConflict:
                        //资源组规则未通过,log提示出来
                        log_str = VFSManagerEditorI18N.Log_ConfigureGroupsConflict;
                        if (folderError != null && folderError.Length > 0)
                        {
                            foreach(var f in folderError)
                            {
                                log_str += $"\nGroup [{f.GroupName}] , FolderPath: {f.FolderPath}";
                            }
                        }
                        Debug.LogError(log_str);
                        return; //直接不继续往下执行了

                    case VFSErrorCode.NoneGroup:
                        //没有配置任何资源组
                        //这个问题不报Error
                        return;

                    case VFSErrorCode.SameGroupName:
                        log_str = VFSManagerEditorI18N.Log_SameGroupName;
                        Debug.LogError(log_str);
                        return;

                }
            }

            Groups.Clear();
            if (!mConfig.EnableWebVFS) return;

            //VFS Profile
            XDirectory.CreateIfNotExists(XEditorConst.EditorProjectSettingRootFolder);
            var profile_path = Path.Combine(XEditorConst.EditorProjectSettingRootFolder, VFSEditorConst.VFSProfileProjectSettingFileName);
            if (File.Exists(profile_path))
            {
                //load
                VFSProfileEditor = XConfig.GetJson<VFSProfileModel>(profile_path, AssetLoadType.SystemIO, false);
            }
            else
            {
                //create profile editor file in "ProjectSetting"
                initProfileRecord();
                //var json_text = JsonUtility.ToJson(VFSProfileEditor);
                XConfig.SaveJson(VFSProfileEditor, profile_path, AssetLoadType.SystemIO);
            }

            foreach(var group_opt in mConfig.Groups)
            {
                var _group_obj = new VFSGroup(group_opt);
                Groups.Add(_group_obj);
                FolderPaths.AddRange(_group_obj.FolderPaths);
                foreach(var assetPath in _group_obj.AssetPaths)
                {
                    if (!AssetPaths.Contains(assetPath))
                    {
                        AssetPaths.Add(assetPath);
                    }
                }
            }

            LastRefreshManagerTime = System.DateTime.UtcNow;
        }

        public static string[] GetAllFolderPaths()
        {
            return FolderPaths?.ToArray() ?? System.Array.Empty<string>();
        }

        public static string[] GetAllWithelistAssetsPaths()
        {
            return AssetPaths?.ToArray() ?? System.Array.Empty<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <param name="simple">如果true，在检查到“不能被VFS管理”的任何一个条件后就停止查询</param>
        /// <returns>如果是“可以被VFS管理”的资源，返回true</returns>
        public static bool QueryAsset(string path , out AssetsStatusQueryResult result , bool simple = false)
        {
            return VFSManagerEditor.QueryAsset(path, mConfig, out result,simple);   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="config"></param>
        /// <param name="result"></param>
        /// <param name="simple">如果true，在检查到“不能被VFS管理”的任何一个条件后就停止查询</param>
        /// <returns>如果是“可以被VFS管理”的资源，返回true</returns>
        public static bool QueryAsset(string path, VFSConfigModel config, out AssetsStatusQueryResult result, bool simple = false)
        {
            result = new AssetsStatusQueryResult();
            result.AssetPath = path;

            if(config == null)
            {
                Debug.LogError("[EDITOR][TinaX.VFS]Can't Query Asset, because config is invalid.");
                return false;
            }

            string[] path_items = path.Split('/');
            string ext = XPath.GetExtension(path, true);

            #region 全局规则判定
            //检查全局规则
            //检查【全局】后缀名
            if (config.GlobalVFS_Ignore_ExtName.Contains(ext))
            {
                //后缀名在忽略列表中
                result.IgnoreByGlobal_IgnoreExtName_List = true;
                if (simple) return false;
            }
            result.IgnoreByGlobal_IgnoreExtName_List = false;

            //检查【全局】忽略Path item
            //path_items.Except(config.GlobalVFS_Ignore_Path_Item) //不用LINQ了，需要忽略大小写
            if(config.GlobalVFS_Ignore_Path_Item != null && config.GlobalVFS_Ignore_Path_Item.Length > 0)
            {
                foreach (var item_a in path_items)
                {
                    string item_a_lower = item_a.ToLower();
                    foreach (var item_b_lower in config.GetGlobalVFS_Ignore_Path_Item())
                    {
                        if(item_b_lower == item_a_lower)
                        {
                            result.IgnoreByGlobal_IgnorePathItem_List = true;
                            if (simple)
                                return false;
                            else
                                break;
                        }
                    }
                }
            }
            result.IgnoreByGlobal_IgnorePathItem_List = false;


            #endregion

            //全局规则对它筛选了一遍，接下来看看它是否属于某个Group
            foreach(var group in Groups)
            {
                if (group.IsAssetPathMatch(path))
                {
                    //这里的查询被简化了，有任何不符合的规则就直接break了，所以其实没有获得细节，后期如果有地方需要细节信息看看再折腾吧
                    result.GroupName = group.GroupName;
                    result.InWhitelist = true;

                    //查询规则
                    var ab_name_noExt = group.GetAssetBundleNameOfAsset(path, out var buildType, out var devType);
                    result.BuildType = buildType;
                    result.DevType = devType;

                    string assetBundleExtension = config.AssetBundleFileExtension;
                    if (!assetBundleExtension.StartsWith("."))
                        assetBundleExtension = "." + assetBundleExtension;

                    result.AssetBundleFileName = ab_name_noExt + assetBundleExtension;
                    result.AssetBundleFileNameWithoutExtension = ab_name_noExt;
                    result.ExtensionGroup = group.ExtensionGroup;

                    break;
                }
                else
                {
                    result.InWhitelist = false;
                }
            }

            return result.ManagedByVFS;
        }

        public static string[] GetGroupNames()
        {
            string[] g = new string[Groups.Count];
            for(var i = 0; i < Groups.Count; i++)
            {
                g[i] = Groups[i].GroupName;
            }
            return g;
        }

        public static string[] GetExtensionGroupNames()
        {
            List<string> names = new List<string>();
            foreach(var item in Groups)
            {
                if (item.ExtensionGroup)
                {
                    names.Add(item.GroupName);
                }
            }
            return names.ToArray();
        }

        public static bool TryGetGroupHandleMode(string groupName,out GroupHandleMode handleMode)
        {
            foreach(var item in Groups)
            {
                if(item.GroupName == groupName)
                {
                    handleMode = item.HandleMode;
                    return true;
                }
            }

            handleMode = default;
            return false;
        }

        public static List<VFSGroup> GetGroups()    //好像直接拿Groups会少很多事，反正class是引用类型
        {
            return Groups;
        }

        public static ProfileRecord GetProfileRecord(string profileName)
        {
            if (VFSProfileEditor.TryGetProfille(profileName,out var pr))
            {
                return pr;
            }
            else
            {
                return new ProfileRecord()
                    .SetProfileName(profileName)
                    .DefaultByGrousp(Groups);
            }
        }


        public static void AddProfileIfNotExists(ProfileRecord pr)
        {
            VFSProfileEditor.AddProfileIfNotExists(pr);
        }



        public static void SaveProfileRecord()
        {
            if(VFSProfileEditor != null)
            {
                VFSProfileEditor.ReadySave();
                var profile_path = Path.Combine(XEditorConst.EditorProjectSettingRootFolder, VFSEditorConst.VFSProfileProjectSettingFileName);
                XConfig.SaveJson(VFSProfileEditor, profile_path, AssetLoadType.SystemIO);
            }
        }

        public static string GetEditorFileServerRootPath()
        {
            return VFSEditorConst.PROJECT_VFS_FILES_SERVER_FOLDER_PATH;
        }

        /// <summary>
        /// 初始值
        /// </summary>
        private static void initProfileRecord()
        {
            if (VFSProfileEditor == null)
                VFSProfileEditor = new VFSProfileModel();
            var profiles = XCoreEditor.GetXProfileNames();
            foreach(var profileName in profiles)
            {
                if(!VFSProfileEditor.TryGetProfille(profileName, out var pr))
                {
                    var profile = new ProfileRecord()
                        .SetProfileName(profileName)
                        .DefaultByGrousp(Groups);
                    VFSProfileEditor.AddProfileIfNotExists(profile);
                }
                
            }
        }



        #region 编辑器菜单

        [MenuItem("Explor VFS Assts Build Folder",menuItem = "TinaX/VFS/Explor VFS Assts Build Folder",priority = 21)]
        static void OpenABBuildFolder()
        {
            var uri = new System.Uri(VFSEditorConst.PROJECT_VFS_FILES_ROOT_FOLDER_PATH);
            Application.OpenURL(uri.ToString());
        }

        #endregion


    }

}

