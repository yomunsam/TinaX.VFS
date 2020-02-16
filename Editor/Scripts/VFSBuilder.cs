using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;
using TinaXEditor.VFSKit.Pipeline;
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

        public void RefreshAssetBundleSign()
        {
            /*
             * Unity目前提供的API对应的打包方法是：
             * 1. 给全局的文件加上assetbundle标记
             * 2. 整体打包
             */

            //获取到所有组的文件白名单目录
            var _whiteLists_folder = VFSManagerEditor.GetAllFolderPaths();
            string ab_extension = Config.AssetBundleFileExtension; //没有点开头的后缀名
            if (ab_extension.StartsWith("."))
                ab_extension = ab_extension.Substring(1, ab_extension.Length - 1);
            string[] guids = AssetDatabase.FindAssets("", _whiteLists_folder);

            foreach(var guid in guids)
            {
                string cur_asset_path = AssetDatabase.GUIDToAssetPath(guid);
                if(VFSManagerEditor.QueryAsset(cur_asset_path, Config, out AssetsStatusQueryResult result, true))
                {
                    //
                    var importer = AssetImporter.GetAtPath(cur_asset_path);
                    if (!XPath.IsFolder(cur_asset_path) && !result.AssetBundleFileName.IsNullOrEmpty())
                    {
                        //正式设置AssetBundle
                        importer.SetAssetBundleNameAndVariant(result.AssetBundleFileName, ab_extension);


                    }
                }
            }

            AssetDatabase.SaveAssets();

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
