using System;
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
using TinaX.IO;


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

        public void RefreshAssetBundleSign(bool recordAssetHash = true)
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

            var _whitelist_folder_temp = _whiteLists_folder;
            for (var i = 0; i < _whitelist_folder_temp.Length; i++)
            {
                if (_whitelist_folder_temp[i].EndsWith("/"))
                    _whitelist_folder_temp[i] = _whitelist_folder_temp[i].Substring(0, _whitelist_folder_temp[i].Length - 1);
            }
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

            if (recordAssetHash) asset_hash_book.Clear();
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

                    if (sign_flag)
                    {
                        var importer = AssetImporter.GetAtPath(cur_asset_path);
                        if (!XPath.IsFolder(cur_asset_path) && !result.AssetBundleFileName.IsNullOrEmpty())
                        {
                            //正式设置AssetBundle
                            importer.SetAssetBundleNameAndVariant(result.AssetBundleFileName, ab_extension);

                            //记录
                            if (recordAssetHash)
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



        private void refreshProfileInfos()
        {
            if (curProfile == null)
                curProfile = VFSManagerEditor.GetProfileRecord(XCoreEditor.GetCurrentActiveXProfileName());
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

    }
}
