using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;
using TinaX.VFSKitInternal;
using TinaXEditor.VFSKit.Pipeline;
using TinaXEditor.VFSKitInternal;
using UnityEngine;
using UnityEditor;
using TinaX;
using TinaX.Utils;
using TinaXEditor.Utils;
using TinaXEditor.VFSKit.Const;
using TinaXEditor.VFSKit.Utils;
using TinaX.IO;
using TinaX.VFSKit.Const;


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
        private List<FilesHashBook.FileHash> asset_hash_book = new List<FilesHashBook.FileHash>();

        private ProfileRecord curProfile;
        private string mProfileName;
        private bool mDevelopMode;

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
            asset_paths = null;
            asset_guids = null;

            asset_hash_book.Clear();
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
                        if (!XPath.IsFolder(cur_asset_path) && !result.AssetBundleFileName.IsNullOrEmpty())
                        {
                            //正式设置AssetBundle
                            importer.SetAssetBundleNameAndVariant(result.AssetBundleFileName, ab_extension);

                            //记录
                            asset_hash_book.Add(new FilesHashBook.FileHash() { p = cur_asset_path, h = XFile.GetMD5(cur_asset_path) });

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
            this.BuildAssetBundle(platform, compressType, out var s1,out var s2);
        }

        public void BuildAssetBundle(XRuntimePlatform platform, AssetCompressType compressType, out string output_folder, out string temp_output_folder)
        {
            var buildTarget = XPlatformEditorUtil.GetBuildTarget(platform);
            var target_name = XPlatformUtil.GetNameText(platform);
            output_folder = Path.Combine(VFSEditorConst.PROJECT_VFS_FILES_ROOT_FOLDER_PATH, target_name);
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

            //叫Unity来打ab包
            BuildPipeline.BuildAssetBundles(build_output_folder, build_opt, buildTarget);

            //打包完了，把所有打包得到的AssetBundle文件记录下来
            string ab_extension = Config.AssetBundleFileExtension;
            if (!ab_extension.StartsWith("."))
                ab_extension = "." + ab_extension;

            string[] files = Directory.GetFiles(build_output_folder, $"*{ab_extension}", SearchOption.AllDirectories);

            var hashBook = new FilesHashBook();
            List<FilesHashBook.FileHash> temp_hash_list = new List<FilesHashBook.FileHash>();
            int build_output_folder_len = build_output_folder.Length + 1;
            foreach (var file in files)
            {
                string pure_path = file.Substring(build_output_folder_len, file.Length - build_output_folder_len);
                if (pure_path.IndexOf("\\") != -1)
                    pure_path = pure_path.Replace("\\", "/");
                temp_hash_list.Add(new FilesHashBook.FileHash()
                {
                    p = pure_path,
                    h = XFile.GetMD5(file)
                });
            }
            hashBook.Files = temp_hash_list.ToArray();
            string hashBook_path = Path.Combine(build_output_folder, VFSConst.ABsHashFileName);
            XFile.DeleteIfExists(hashBook_path);
            string hashBook_json = JsonUtility.ToJson(hashBook);
            File.WriteAllText(hashBook_path, hashBook_json, Encoding.UTF8);
        }

        public void Build(XRuntimePlatform platform, AssetCompressType compressType)
        {
            Debug.Log("[TinaX.VFS] Start build assets...");
            if (ClearAssetBundleSignBeforeBuild)
                VFSEditorUtil.RemoveAllAssetbundleSigns();
            Debug.Log("    handle assetbundle signs...");
            RefreshAssetBundleSign();
            Debug.Log("    build assetbundles by unity editor.");

            string output_root_path;    //VFS存放相关文件的根目录
            string output_temp_path;    //Unity自身打包后直接输出的目录
            BuildAssetBundle(platform, compressType, out output_root_path, out output_temp_path);

            HandleVFSFiles(output_root_path, output_temp_path);

            if (ClearAssetBundleSignAfterBuild)
                VFSEditorUtil.RemoveAllAssetbundleSigns();

            Debug.Log("[TinaX.VFS] Build Finished.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root_path">存放一系列VFS目录的根目录</param>
        /// <param name="build_root_path">Unity的Build结果输出目录</param>
        public void HandleVFSFiles(string root_path, string build_root_path)
        {
            List<VFSGroup> groups = VFSManagerEditor.GetGroups(); 
            foreach(var group in groups)
            {
                //扩展组的判断
                if (group.ExtensionGroup)
                {
                    #region 处理扩展组
                    string extension_group_root_path = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILES_FOLDER_EXTENSION, group.GroupName.ToLower());
                    XDirectory.CreateIfNotExists(extension_group_root_path); //TODO 这儿写错了
                    MoveAssetBundleFilesByGroup(group, build_root_path, extension_group_root_path);
                    #endregion
                }
                else
                {
                    #region 移走可能的Remote
                    if(group.HandleMode == GroupHandleMode.LocalOrRemote || group.HandleMode == GroupHandleMode.RemoteOnly)
                    {
                        if(curProfile.TryGetGroupLocation(group.GroupName,out var location))
                        {
                            if(location == ProfileRecord.E_GroupAssetsLocation.Remote)
                            {
                                string remote_group_root_path = Path.Combine(root_path, VFSEditorConst.PROJECT_VFS_FILE_FOLDER_REMOTE, group.GroupName.ToLower());
                                XDirectory.CreateIfNotExists(remote_group_root_path);
                                MoveAssetBundleFilesByGroup(group, build_root_path, remote_group_root_path);
                            }
                        }
                    }

                    #endregion
                }

            }
        }


        public void CopyToStreamingAssets()
        {

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
        /// 根据Group的配置，把构建好的AssetBundle文件中的相关文件移动到指定的地方
        /// </summary>
        /// <param name="group"></param>
        /// <param name="build_ab_path"></param>
        /// <returns></returns>
        private void MoveAssetBundleFilesByGroup(VFSGroup group, string build_ab_path,string target_root_path)
        {
            string extension = Config.AssetBundleFileExtension;
            if (!extension.StartsWith("."))
                extension = "." + extension;
            foreach(var folder in group.FolderPathsLower)
            {
                string full_folder_path = Path.Combine(build_ab_path, folder);
                if (Directory.Exists(full_folder_path))
                {
                    XDirectory.CopyDir(full_folder_path, target_root_path);
                    Directory.Delete(full_folder_path,true);
                }
            }
            foreach(var asset in group.FolderPathsLower)
            {
                string ab_name = group.GetAssetBundleNameOfAsset(asset) + extension;
                string ab_full_name = Path.Combine(build_ab_path, ab_name);
                string move_target_name = Path.Combine(target_root_path, ab_name);
                if (File.Exists(ab_full_name))
                {
                    if (File.Exists(move_target_name))
                        File.Delete(move_target_name);

                    File.Move(ab_full_name, move_target_name);
                }
            }
        }
    }
}
