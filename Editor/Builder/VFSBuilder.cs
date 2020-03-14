using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using TinaX;
using TinaX.IO;
using TinaX.Utils;
using TinaX.VFSKit;
using TinaX.VFSKit.Const;
using TinaX.VFSKitInternal;
using TinaX.VFSKitInternal.Utils;
using TinaXEditor.Utils;
using TinaXEditor.VFSKit.Pipeline;
using TinaXEditor.VFSKit.Const;
using TinaXEditor.VFSKit.Utils;
using TinaXEditor.VFSKitInternal;
using UnityEditor;
using UnityEngine;


namespace TinaXEditor.VFSKit
{
    /// <summary>
    /// VFS Assets Builder
    /// </summary>
    public class VFSBuilder : IVFSBuilder
    {
        public VFSConfigModel Config { get; private set; }

        /// <summary>
        /// Show tips GUI in editor..
        /// </summary>
        public bool EnableTipsGUI { get; set; } = false;

        public bool CopyToStreamingAssetsFolder { get; set; } = false;
        public bool StrictMode { get; set; } = false;
        public bool ClearAssetBundleSignBeforeBuild { get; set; } = false;
        public bool ClearAssetBundleSignAfterBuild { get; set; } = false;

        /// <summary>
        /// Force Rebuild | 强制重构建
        /// </summary>
        public bool ForceRebuild { get; set; } = false;

        public bool ClearOutputFolder { get; set; } = false;

        //private HashSet<string[]>
        private List<FilesHashBook.FileHash> asset_hash_book = new List<FilesHashBook.FileHash>(); //记录的是unity工程里原始的asset，而不是打包后的ab
        private Dictionary<string, List<FilesHashBook.FileHash>> dict_asset_hash_book = new Dictionary<string, List<FilesHashBook.FileHash>>(); //和上面一样存储的是原始asset, 不过这里是针对扩展组的，key的组名
        /// <summary>
        /// 按照组，把每个组中包含的AssetBundle名存下来
        /// </summary>
        private Dictionary<string, List<string>> mDict_Group_AssetBundleNames = new Dictionary<string, List<string>>(); //按照组，把每个组中包含的AssetBundle名存下来

        private ProfileRecord curProfile;
        private string mProfileName;
        private bool mDevelopMode;
        private AssetBundleManifest mAssetBundleManifest;

        private BuilderPipeline mPipeline;
        private bool mUsePipeline = false;

        private EditorBuildInfo mEditorBuildInfo;

        public VFSBuilder()
        {

        }

        public VFSBuilder(VFSConfigModel config)
        {
            Config = config;
        }

        public IVFSBuilder SetConfig(VFSConfigModel config)
        {
            Config = config;
            return this;
        }

        public IVFSBuilder UseProfile(string profileName)
        {
            mProfileName = profileName;
            curProfile = VFSManagerEditor.GetProfileRecord(profileName);
            mDevelopMode = XCoreEditor.IsXProfileDevelopMode(mProfileName);
            return this;
        }

        public IVFSBuilder UsePipeline(BuilderPipeline pipeline)
        {
            mUsePipeline = true;
            mPipeline = pipeline;
            return this;
        }

        public IVFSBuilder UseAutoPipeline()
        {
            var interface_type = typeof(IBuildHandler);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(interface_type) && t != typeof(TinaXEditor.VFSKit.Pipeline.Builtin.BuilderPipelineHead) && t != typeof(TinaXEditor.VFSKit.Pipeline.Builtin.BuilderPipelineLast)))
                .ToArray();
            Dictionary<Type, int> dict_priority = new Dictionary<Type, int>();
            //查找优先级
            foreach(var type in types)
            {
                var priority_attr = type.GetCustomAttribute<TinaX.PriorityAttribute>(true);
                if(priority_attr == null)
                {
                    dict_priority.Add(type, 100);
                }
                else
                {
                    dict_priority.Add(type, priority_attr.Priority);
                }
            }

            List<Type> list_types = new List<Type>(types);
            list_types.Sort((x, y) => dict_priority[x].CompareTo(dict_priority[y]));

            var pipeline = new BuilderPipeline();
            foreach (var type in list_types)
            {
                pipeline.AddLast(Activator.CreateInstance(type) as IBuildHandler);
            }
            this.mUsePipeline = true;
            this.mPipeline = pipeline;

            return this;
        }

        public void RefreshAssetBundleSign()
        {
            /*
             * Unity目前提供的API对应的打包方法是：
             * 1. 给全局的文件加上assetbundle标记
             * 2. 整体打包
             */

            //获取到所有组的文件白名单目录
            refreshProfileInfos();
            if (EnableTipsGUI) EditorUtility.DisplayProgressBar("VFS Builder", "Handle AssetBundle signs ...", 0f);

            var _whiteLists_folder = VFSManagerEditor.GetAllFolderPaths();
            string ab_extension = Config.AssetBundleFileExtension; //没有点开头的后缀名
            if (ab_extension.StartsWith("."))
                ab_extension = ab_extension.Substring(1, ab_extension.Length - 1);
            ab_extension = ab_extension.ToLower();
            List<string> _whiteLists_folder_list_temp = new List<string>();
            foreach(var path in _whiteLists_folder)
            {
                if (!IsAssetFolderExists(path))
                {
                    Debug.LogError("[TinaX][VFS]Folder path in vfs config is not exists: \"" + path + "\"");
                }
                else
                {
                    if (path.EndsWith("/"))
                        _whiteLists_folder_list_temp.Add(path.Substring(0, path.Length - 1));
                    else
                        _whiteLists_folder_list_temp.Add(path);
                }
            }
            var _whitelist_folder_temp = _whiteLists_folder_list_temp.ToArray();

            string[] guids = AssetDatabase.FindAssets("", _whitelist_folder_temp);

            string[] asset_paths = VFSManagerEditor.GetAllWithelistAssetsPaths();
            List<string> asset_guids = new List<string>();
            foreach(var item in asset_paths)
            {
                var myguid = AssetDatabase.AssetPathToGUID(item);
                if (!myguid.IsNullOrEmpty())
                    asset_guids.Add(myguid);
            }
            if (asset_guids.Count > 0)
                ArrayUtil.Combine(ref guids, asset_guids.ToArray());
            ArrayUtil.RemoveDuplicationElements(ref guids);
            asset_paths = null;
            asset_guids = null;

            asset_hash_book.Clear();
            dict_asset_hash_book.Clear();
            int counter = 0;
            int counter_t = 0;
            int totalLength = guids.Length;
            foreach (var guid in guids)
            {
                string cur_asset_path = AssetDatabase.GUIDToAssetPath(guid);
                if (VFSManagerEditor.QueryAsset(cur_asset_path, Config, out AssetsStatusQueryResult result, true))
                {
                    //查询到了信息，但是并不是所有情况都需要设置assetbundle记录，
                    bool sign_flag = true;
                    if (result.DevType == FolderBuildDevelopType.editor_only)
                        sign_flag = false; //该资源仅在编辑器下被加载，不参与打包。

                    if(result.DevType == FolderBuildDevelopType.develop_mode_only)
                    {
                        if (!mDevelopMode)
                            sign_flag = false; //资源应该仅在develop模式下使用，但是当前Profile的设置并不是develop
                    }

                    if (result.ExtensionGroup)
                        if (curProfile.IsDisabledGroup(result.GroupName))
                            sign_flag = false;

                    if (sign_flag)
                    {
                        var importer = AssetImporter.GetAtPath(cur_asset_path);
                        if (!XPath.IsFolder(cur_asset_path) && !result.AssetBundleFileNameWithoutExtension.IsNullOrEmpty())
                        {
                            string assetBundleName_without_extension = result.AssetBundleFileNameWithoutExtension;
                            void InvokePipeline(BuilderPipelineContext ctx)
                            {
                                if(ctx != null && ctx.Handler != null)
                                {
                                    bool b = ctx.Handler.BeforeSetAssetBundleSign(ref assetBundleName_without_extension, ref result);
                                    if(b && ctx.Next != null)
                                    {
                                        InvokePipeline(ctx.Next);
                                    }
                                }
                            }
                            if (mUsePipeline)
                            {
                                InvokePipeline(mPipeline.First);
                            }

                            //正式设置AssetBundle
                            importer.SetAssetBundleNameAndVariant(assetBundleName_without_extension, ab_extension);

                            //记录Asset原始hash（用于补丁版本检索）
                            if (result.ExtensionGroup)
                            {
                                //记录到字典里
                                if (!dict_asset_hash_book.ContainsKey(result.GroupName))
                                    dict_asset_hash_book.Add(result.GroupName, new List<FilesHashBook.FileHash>());
                                dict_asset_hash_book[result.GroupName].Add(new FilesHashBook.FileHash() { p = cur_asset_path, h = XFile.GetMD5(cur_asset_path, true) });
                            }
                            else
                            {
                                asset_hash_book.Add(new FilesHashBook.FileHash() { p = cur_asset_path, h = XFile.GetMD5(cur_asset_path, true) });
                            }
                            //记录AssetBundle名
                            string final_ab_name = assetBundleName_without_extension + "." + ab_extension;
                            if (!mDict_Group_AssetBundleNames.ContainsKey(result.GroupName))
                                mDict_Group_AssetBundleNames.Add(result.GroupName, new List<string>());
                            if (!mDict_Group_AssetBundleNames[result.GroupName].Contains(final_ab_name))
                                mDict_Group_AssetBundleNames[result.GroupName].Add(final_ab_name);

                        }
                    }
                }

                if (EnableTipsGUI)
                {
                    counter++;
                    if(totalLength < 100)
                    {
                        EditorUtility.DisplayProgressBar("VFS Builder", $"Handle AssetBundle signs : {counter} / {totalLength}", counter / totalLength);
                    }
                    else
                    {
                        counter_t++;
                        if(counter_t > 50)
                        {
                            counter_t = 0;
                            EditorUtility.DisplayProgressBar($"VFS Builder", "Handle AssetBundle signs : {counter} / {totalLength}", counter / totalLength);
                        }
                    }
                }
            
            }


            AssetDatabase.SaveAssets();
            if (EnableTipsGUI) EditorUtility.ClearProgressBar();


        }

        public void BuildAssetBundle(XRuntimePlatform platform, AssetCompressType compressType)
        {
            this.BuildAssetBundle(platform, compressType, out _, out _);
        }

        public void BuildAssetBundle(XRuntimePlatform platform, AssetCompressType compressType, out string output_folder, out string temp_output_folder)
        {
            var buildTarget = XPlatformEditorUtil.GetBuildTarget(platform);
            var target_name = XPlatformUtil.GetNameText(platform);
            output_folder = Path.Combine(VFSEditorConst.PROJECT_VFS_SOURCE_PACKAGES_ROOT_PATH, target_name);
            string build_output_folder = Path.Combine(output_folder, "build_temp");
            temp_output_folder = build_output_folder;

            if (ClearOutputFolder)
                XDirectory.DeleteIfExists(output_folder, true);
            XDirectory.CreateIfNotExists(output_folder);
            XDirectory.CreateIfNotExists(temp_output_folder);

            //压缩方法
            BuildAssetBundleOptions build_opt = BuildAssetBundleOptions.None;
            switch (compressType)
            {
                default:
                case AssetCompressType.LZ4:
                    build_opt = BuildAssetBundleOptions.ChunkBasedCompression;
                    break;
                case AssetCompressType.LZMA:
                    build_opt = BuildAssetBundleOptions.None;
                    break;
                case AssetCompressType.None:
                    build_opt = BuildAssetBundleOptions.UncompressedAssetBundle;
                    break;
            }

            //强制重新构建
            if (ForceRebuild)
                build_opt = build_opt | BuildAssetBundleOptions.ForceRebuildAssetBundle;
            //严格模式
            if (StrictMode)
                build_opt = build_opt | BuildAssetBundleOptions.StrictMode;
            //Hash保持一致
            build_opt = build_opt | BuildAssetBundleOptions.DeterministicAssetBundle;

            //叫Unity来打ab包
            BuildPipeline.BuildAssetBundles(build_output_folder, build_opt, buildTarget);

            //打包完了，把所有打包得到的AssetBundle文件记录下来
            string ab_extension = Config.AssetBundleFileExtension;
            if (!ab_extension.StartsWith("."))
                ab_extension = "." + ab_extension;

            string[] files = Directory.GetFiles(build_output_folder, $"*{ab_extension}", SearchOption.AllDirectories);

            #region temp下的目录完全hash
            //var hashBook = new FilesHashBook();
            //List<FilesHashBook.FileHash> temp_hash_list = new List<FilesHashBook.FileHash>();
            //int build_output_folder_len = build_output_folder.Length + 1;
            //foreach (var file in files)
            //{
            //    string pure_path = file.Substring(build_output_folder_len, file.Length - build_output_folder_len);
            //    if (pure_path.IndexOf("\\") != -1)
            //        pure_path = pure_path.Replace("\\", "/");
            //    temp_hash_list.Add(new FilesHashBook.FileHash()
            //    {
            //        p = pure_path,
            //        h = XFile.GetMD5(file, true)
            //    });
            //}
            //hashBook.Files = temp_hash_list.ToArray();
            //string hashBook_path = Path.Combine(build_output_folder, VFSConst.ABsHashFileName);
            //XFile.DeleteIfExists(hashBook_path);
            //string hashBook_json = JsonUtility.ToJson(hashBook);
            //File.WriteAllText(hashBook_path, hashBook_json, Encoding.UTF8);
            #endregion
        }

        public void Build(XRuntimePlatform platform, AssetCompressType compressType)
        {
            Debug.Log("[TinaX.VFS] Start build assets...");
            if (ClearAssetBundleSignBeforeBuild)
                VFSEditorUtil.RemoveAllAssetbundleSigns();
            Debug.Log("    handle assetbundle signs...");
            RefreshAssetBundleSign();
            Debug.Log("    build assetbundles by unity editor.");

            string packages_root_path;    //VFS存放相关文件的根目录
            string output_temp_path;    //Unity自身打包后直接输出的目录
            BuildAssetBundle(platform, compressType, out packages_root_path, out output_temp_path);

            #region 加载Build得到的AssetbundleManifest文件
            string abmanifest_file_name = Path.GetFileName(output_temp_path);
            string abmanifest_path = Path.Combine(output_temp_path, abmanifest_file_name);
            AssetBundle ab_mainifest = AssetBundle.LoadFromFile(abmanifest_path);
            mAssetBundleManifest = ab_mainifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            #endregion

            HandleVFSFiles(packages_root_path, output_temp_path, platform);
            SaveAssetHashFiles(Path.Combine(packages_root_path, VFSEditorConst.PROJECT_VFS_FILE_FOLDER_DATA));
            MakeEditorBuildInfo(packages_root_path);
            MakeBuildInfo(packages_root_path);
            MakeVFSConfig(packages_root_path,Config);

            if (CopyToStreamingAssetsFolder)
                CopyToStreamingAssets(packages_root_path,XPlatformUtil.GetNameText(platform));

            if (ClearAssetBundleSignAfterBuild)
                VFSEditorUtil.RemoveAllAssetbundleSigns();

            Debug.Log("[TinaX.VFS] Build Finished.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root_path">存放一系列VFS目录的根目录</param>
        /// <param name="build_root_path">Unity的Build结果输出目录</param>
        public void HandleVFSFiles(string root_path, string build_root_path,XRuntimePlatform platform)
        {
            mEditorBuildInfo = new EditorBuildInfo();
            List<VFSEditorGroup> groups = VFSManagerEditor.GetGroups();
            string remote_files_root_path = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILE_FOLDER_REMOTE);
            string local_files_root_path = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILES_FOLDER_MAIN);
            string source_packages_extension_root_folder = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILES_FOLDER_EXTENSION);

            XDirectory.DeleteIfExists(remote_files_root_path, true);
            XDirectory.DeleteIfExists(local_files_root_path, true);
            XDirectory.DeleteIfExists(source_packages_extension_root_folder, true);

            foreach (var group in groups)
            {
                //扩展组的判断
                if (group.ExtensionGroup)
                {
                    #region 处理扩展组
                    string extension_group_root_path = Path.Combine(source_packages_extension_root_folder, group.GroupName.ToLower());
                    bool moved = CopyAssetBundleFilesByGroup(group, build_root_path, extension_group_root_path);
                    if (moved)
                    {
                        //manifest
                        group.MakeVFSManifest(root_path, mDict_Group_AssetBundleNames[group.GroupName], ref mAssetBundleManifest);
                        //给独立的组生成一份hash
                        group.MakeAssetBundleFilesHash(root_path, extension_group_root_path, mDict_Group_AssetBundleNames[group.GroupName]);
                        SaveExtensionGroupInfo(extension_group_root_path, group.GroupName, platform, group.ExtensionGroup_MainPackageVersionLimit,Config.AssetBundleFileExtension);
                        //保存Options
                        group.SaveGroupOptionFile(root_path);
                    }

                    #endregion

                    //登记
                    if (curProfile.IsDisabledGroup(group.GroupName))
                        mEditorBuildInfo.list_disable_extension.Add(group.GroupName);
                    else
                    {
                        mEditorBuildInfo.list_total_extension.Add(group.GroupName);

                        if (group.HandleMode == GroupHandleMode.LocalOrRemote)
                        {
                            if (curProfile.TryGetGroupLocation(group.GroupName, out var location))
                            {
                                if (location == ProfileRecord.E_GroupAssetsLocation.Local)
                                    mEditorBuildInfo.list_extension_local.Add(group.GroupName);
                                else if (location == ProfileRecord.E_GroupAssetsLocation.Remote)
                                    mEditorBuildInfo.list_extension_remote.Add(group.GroupName);
                            }
                            else
                                mEditorBuildInfo.list_extension_local.Add(group.GroupName);

                        }
                        else if (group.HandleMode == GroupHandleMode.RemoteOnly)
                            mEditorBuildInfo.list_extension_remote.Add(group.GroupName);
                        else
                            mEditorBuildInfo.list_extension_local.Add(group.GroupName);
                    }
                    

                }
                else
                {
                    bool moveToRemote = false;
                    bool moveToLocal = false;

                    if (group.HandleMode == GroupHandleMode.LocalOrRemote)
                    {
                        if (curProfile.TryGetGroupLocation(group.GroupName, out var location))
                        {
                            if (location == ProfileRecord.E_GroupAssetsLocation.Remote)
                                moveToRemote = true;
                            else
                                moveToLocal = true;
                        }
                        else
                            moveToLocal = true;
                    }
                    else if (group.HandleMode == GroupHandleMode.RemoteOnly)
                        moveToRemote = true;
                    else if (group.HandleMode == GroupHandleMode.LocalOnly || group.HandleMode == GroupHandleMode.LocalAndUpdatable)
                        moveToLocal = true;

                    //登记
                    mEditorBuildInfo.list_total_main_package.Add(group.GroupName);

                    if (moveToRemote)
                    {
                        //登记
                        mEditorBuildInfo.list_main_package_remote.Add(group.GroupName);
                        XDirectory.CreateIfNotExists(remote_files_root_path);
                        CopyAssetBundleFilesByGroup(group, build_root_path, remote_files_root_path);
                        //上一步copy的时候，开发者自定义的pipeline可以改变文件，所以制作assetbundle的hash信息的时候，必须使用上一步处理后的结果
                        group.MakeAssetBundleFilesHash(root_path, remote_files_root_path, mDict_Group_AssetBundleNames[group.GroupName]);
                    }
                    else if (moveToLocal)
                    {
                        //登记
                        mEditorBuildInfo.list_main_package_local.Add(group.GroupName);
                        XDirectory.CreateIfNotExists(local_files_root_path);
                        CopyAssetBundleFilesByGroup(group, build_root_path, local_files_root_path);
                        //上一步copy的时候，开发者自定义的pipeline可以改变文件，所以制作assetbundle的hash信息的时候，必须使用上一步处理后的结果
                        group.MakeAssetBundleFilesHash(root_path, local_files_root_path, mDict_Group_AssetBundleNames[group.GroupName]); 
                    }


                    group.MakeVFSManifest(root_path, mDict_Group_AssetBundleNames[group.GroupName], ref mAssetBundleManifest);
                }

            }

        }

        private void SaveAssetHashFiles(string data_path)
        {
            XDirectory.CreateIfNotExists(data_path);
            //mainPackage
            string mainPackageHashPath = Path.Combine(data_path, VFSConst.AssetsHashFileName);
            var main_obj = new FilesHashBook();
            main_obj.Files = asset_hash_book.ToArray();
            XConfig.SaveJson(main_obj, mainPackageHashPath, AssetLoadType.SystemIO);

            string extGroupHashFolderPath = Path.Combine(data_path, VFSConst.ExtensionGroupAssetsHashFolderName);
            XDirectory.DeleteIfExists(extGroupHashFolderPath,true);
            Directory.CreateDirectory(extGroupHashFolderPath);
            //各个扩展组
            foreach(var item in dict_asset_hash_book)
            {
                string g_path = Path.Combine(extGroupHashFolderPath, item.Key + ".json");
                var obj = new FilesHashBook();
                obj.Files = item.Value.ToArray();
                XConfig.SaveJson(obj, g_path, AssetLoadType.SystemIO);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root_path">root_path下面应该就是"vfs_root","vfs_data"之类的文件</param>
        public void CopyToStreamingAssets(string root_path,string platform_name)
        {
            Debug.Log("    copy To \"StreamingAssets\"");
            VFSEditorUtil.CopyToStreamingAssets(root_path, platform_name);
        }

        private void SaveExtensionGroupInfo(string group_path, string group_name, XRuntimePlatform platform , long mainPackageVersionLimit, string ab_ext_name)
        {
            string file_path = VFSUtil.GetExtensionGroup_GroupInfo_Path_InGroupPath(group_path);
            var obj = new ExtensionGroupInfo
            {
                Platform = platform,
                GroupName = group_name,
                MainPackageVersionLimit = mainPackageVersionLimit,
                AssetBundleExtension = ab_ext_name
            };
            XConfig.SaveJson(obj, file_path, AssetLoadType.SystemIO);
        }

        private void refreshProfileInfos()
        {
            if (curProfile == null)
                curProfile = VFSManagerEditor.GetProfileRecord(XCoreEditor.GetCurrentActiveXProfileName());
        }

        private bool IsAssetFolderExists(string path)
        {
            if (!path.StartsWith("Assets/"))
                return false;

            return System.IO.Directory.Exists(System.IO.Path.GetFullPath(path));
        }

        /// <summary>
        /// 不要处理的后缀
        /// </summary>
        private readonly string[] DontHandle_Ext =
        {
            ".cs",
            ".dll",
            ".so",
            ".exe",
            ".apk",
            ".ipa"
        };

        private bool ExtCanHandle(string path)//如果后缀名可以被处理，返回true
        {
            var path_ext_name = System.IO.Path.GetExtension(path).ToLower();
            return !DontHandle_Ext.Any(item => item == path_ext_name.ToLower());
        }


        
        
        /// <summary>
        /// 把构建好的AssetBundle文件根据组进行复制分类
        /// </summary>
        /// <param name="group"></param>
        /// <param name="build_ab_path"></param>
        /// <param name="target_root_path"></param>
        /// <returns></returns>
        private bool CopyAssetBundleFilesByGroup(VFSEditorGroup group, string build_ab_path, string target_root_path)
        {
            bool moved = false;
            int counter = 0;
            int counter_t = 0;
            int total_len = mDict_Group_AssetBundleNames[group.GroupName].Count;
            if (EnableTipsGUI && total_len > 100)
            {
                EditorUtility.DisplayProgressBar("Classifying files", $"Classifying assetbundle files by group \"{group.GroupName}\"", 0.5f);
            }
            foreach (var assetbundle_name in mDict_Group_AssetBundleNames[group.GroupName])
            {
                if (EnableTipsGUI)
                {
                    counter++;
                    counter_t++;
                    if (total_len > 100)
                    {
                        if (counter_t > 50)
                        {
                            counter_t = 0;
                            EditorUtility.DisplayProgressBar("Classifying files", $"Classifying assetbundle files by group \"{group.GroupName}\"\n{counter} / {total_len}", counter/total_len);
                        }
                    }
                }
                string assetbundle_path = Path.Combine(build_ab_path, assetbundle_name);
                if (File.Exists(assetbundle_path))
                {
                    string target_path = Path.Combine(target_root_path, assetbundle_name);
                    XFile.DeleteIfExists(target_path);
                    XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));

                    File.Copy(assetbundle_path, target_path);

                    if (mUsePipeline)
                    {
                        FileStream fileStream = new FileStream(target_path, FileMode.Open, FileAccess.ReadWrite);
                        void InvokePipline(BuilderPipelineContext ctx)
                        {
                            if (ctx.Handler != null)
                            {
                                bool b = ctx.Handler.BeforeAssetBundleFileSavedByGroup(ref group, assetbundle_name, ref fileStream);
                                if (b && ctx.Next != null)
                                {
                                    InvokePipline(ctx.Next);
                                }
                            }
                        }
                        if (mPipeline.First != null)
                        {
                            InvokePipline(mPipeline.First);
                        }
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                    moved = true;
                }
            }
            if (EnableTipsGUI)
                EditorUtility.ClearProgressBar();
            return moved;
        }
        
        /// <summary>
        /// 保存BuildInfo
        /// </summary>
        /// <returns></returns>
        private void MakeBuildInfo(string packages_root_path)
        {
            var binfo = new BuildInfo();
            binfo.BuildID = System.Guid.NewGuid().ToString();

            //save main package
            string main_path = VFSUtil.GetMainPackage_BuildInfo_Path(packages_root_path);
            XFile.DeleteIfExists(main_path);
            XDirectory.CreateIfNotExists(Path.GetDirectoryName(main_path));
            XConfig.SaveJson(binfo, main_path, AssetLoadType.SystemIO);

            //groups
            if(mEditorBuildInfo.list_total_extension != null && mEditorBuildInfo.list_total_extension.Count > 0)
            {
                foreach (var group in mEditorBuildInfo.list_total_extension)
                {
                    string target_path = VFSUtil.GetExtensionGroup_BuildInfo_Path(packages_root_path, group);
                    string group_root_path = VFSUtil.GetExtensionGroupFolder(packages_root_path, group);
                    if (Directory.Exists(group_root_path))
                    {
                        XFile.DeleteIfExists(target_path);
                        XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));
                        XConfig.SaveJson(binfo, target_path, AssetLoadType.SystemIO);
                    }
                }
            }
        }
        
        /// <summary>
        /// 保存EditorBuildInfo
        /// </summary>
        /// <param name="package_root_path"></param>
        private void MakeEditorBuildInfo(string package_root_path)
        {
            mEditorBuildInfo.build_profile_name = curProfile.ProfileName;
            mEditorBuildInfo.ReadySave();
            string path = VFSEditorUtil.Get_EditorBuildInfoPath(package_root_path);
            XFile.DeleteIfExists(path);
            XDirectory.CreateIfNotExists(Path.GetDirectoryName(path));
            XConfig.SaveJson(mEditorBuildInfo, path, AssetLoadType.SystemIO);
        }

        private void MakeVFSConfig(string packages_root_path,VFSConfigModel config)
        {
            string config_path = VFSUtil.GetVFSConfigFilePath_InPackages(packages_root_path);
            string json = JsonUtility.ToJson(config);
            var json_obj = JsonUtility.FromJson<VFSConfigJson>(json);
            if (json_obj.Groups != null)
            {
                List<VFSGroupOption> options = new List<VFSGroupOption>(json_obj.Groups);
                for(int i = options.Count -1; i >= 0; i--)
                {
                    if(options[i].ExtensionGroup)
                    {
                        options.RemoveAt(i);
                        continue;
                    }
                }
                json_obj.Groups = options.ToArray();
            }

            XConfig.SaveJson(json_obj, config_path, AssetLoadType.SystemIO);
        }

    }
}
