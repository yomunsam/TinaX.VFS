using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX;
using TinaX.IO;
using TinaX.Services;
using TinaX.VFSKit.Const;
using TinaX.VFSKit.Exceptions;
using TinaX.VFSKitInternal;
using System;
using UnityEngine;
using UnityEngine.Networking;
using TinaX.VFSKitInternal.Utils;
using UniRx;
using UniRx.Async;
using FileNotFoundException = TinaX.VFSKit.Exceptions.FileNotFoundException;

namespace TinaX.VFSKit
{
    public class VFSKit : IVFS, IVFSInternal, IAssetService
    {

        public string ConfigPath { get; set; } = VFSConst.ConfigFilePath_Resources;
        public AssetLoadType ConfigLoadType { get; private set; } = AssetLoadType.Resources;

        public string VirtualDiskPath { get; private set; }

        public VFSCustomizable Customizable { get; private set; }

        public bool Override_StreamingAssetsPath { get; private set; } = false;

        private string mVirtualDisk_MainPackageFolderPath;

        private bool? _ischinese;
        private bool IsChinese
        {
            get
            {
                if(_ischinese == null)
                {
                    _ischinese = (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified);
                }
                return _ischinese.Value;
            }
        }

        /// <summary>
        /// StreamingAssets -> vfs_root
        /// </summary>
        private string mStreamingAssets_MainPackageFolderPath;   //因为这个地址可能会被重写，所以必须要从这里读取。
        /// <summary>
        /// StreamingAssets -> vfs_extension
        /// </summary>
        private string mStreamingAssets_ExtensionGroupRootFolderPath;   //因为这个地址可能会被重写，所以必须要从这里读取。
        /// <summary>
        /// StreamingAssets -> vfs_data
        /// </summary>
        private string mStreamingAssets_DataRootFolderPath;   //因为这个地址可能会被重写，所以必须要从这里读取。


#if UNITY_EDITOR
        /// <summary>
        /// Editor下，使用UnityEditor.AssetDatabase 加载资源
        /// </summary>
        private bool mLoadByAssetDatabaseInEditor = false;
#endif

        private VFSConfigModel mConfig;

        /// <summary>
        /// 所有组的对象
        /// </summary>
        private List<VFSGroup> mGroups = new List<VFSGroup>();

        /// <summary>
        /// 白名单文件夹路径 | 全局所有Groups里的路径都存放在这里
        /// </summary>
        //private List<string> mWhitelistFolderPaths = new List<string>();

        private VFSException mStartException;

        private XAssetBundleManifest mManifest_StreamingAssets;
        private XAssetBundleManifest mManifest_VirtualDisk;
        private XAssetBundleManifest mAssetBundleManifest
        {
            get
            {
                if (mManifest_VirtualDisk == null) return mManifest_StreamingAssets;
                return mManifest_VirtualDisk;
            }
        }

        private FilesHashBook mFileHash_StreamingAssets;

        internal GetWebAssetUrlDelegate GetWebAssetUrl;

        public VFSKit()
        {
            Customizable = new VFSCustomizable(this);
            this.GetWebAssetUrl = getWebAssetUrl;

            mStreamingAssets_MainPackageFolderPath = VFSUtil.GetMainPackageFolderInStreamingAssets();
            mStreamingAssets_DataRootFolderPath = VFSUtil.GetDataFolderInStreamingAssets();
            mStreamingAssets_ExtensionGroupRootFolderPath = VFSUtil.GetExtensionGroupRootFolderInStreamingAssets();

#if UNITY_EDITOR
            //load mode
            var loadMode = VFSLoadModeInEditor.GetLoadMode();
            switch (loadMode)
            {
                case RuntimeAssetsLoadModeInEditor.LoadByAssetDatabase:
                    mLoadByAssetDatabaseInEditor = true;
                    break;
                case RuntimeAssetsLoadModeInEditor.Normal:
                    mLoadByAssetDatabaseInEditor = false;
                    break;
                case RuntimeAssetsLoadModeInEditor.Override_StreamingAssetsPath:
                    Override_StreamingAssetsPath = true;
                    mStreamingAssets_MainPackageFolderPath = VFSLoadModeInEditor.Get_Override_MainPackagePath();
                    mStreamingAssets_ExtensionGroupRootFolderPath = VFSLoadModeInEditor.Get_Override_ExtensionGroupRootFolderPath();
                    break;
            }
#endif
        }

        /// <summary>
        /// 启动，如果初始化失败，则返回false.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {

            #region Configs
            // load config by xconfig | VFS not ready, so vfs config can not load by vfs.
            mConfig = XConfig.GetConfig<VFSConfigModel>(ConfigPath);
            if (mConfig == null)
            {
                mStartException = new VFSException("Load VFS config failed, \nload type:" + ConfigLoadType.ToString() + "\nload path:" + ConfigPath, VFSErrorCode.LoadConfigFailed);
                return false;
            }
            VFSUtil.NormalizationConfig(ref mConfig);

            if (!VFSUtil.CheckConfiguration(ref mConfig, out var errorCode, out var folderError))
            {
                mStartException = new VFSException("VFS Config Error:", errorCode);
                return false;
            }


            // init configs data.
            mGroups.Clear();
            if (mConfig.Groups != null)
            {
                foreach (var groupOption in mConfig.Groups)
                {
                    var group = new VFSGroup(groupOption);
                    mGroups.Add(group);

                    //init each group status.
                }
            }

            #endregion

            //init vfs virtual disk folder
            VirtualDiskPath = Path.Combine(Application.persistentDataPath, "VFS_VDisk"); //TODO: 在Windows之类目录权限比较自由的平台，未来可以考虑搞个把这个目录移动到别的地方的功能。（毕竟有人不喜欢把太多文件扔在C盘）
            XDirectory.CreateIfNotExists(VirtualDiskPath);

            //init vfs packages in streamingassets
                //main package 's assetbundleManifest
            #region StramingAssets AssetBundleManifest
            string streaming_manifest_path = VFSUtil.GetAssetBundleManifestInPackage(mStreamingAssets_MainPackageFolderPath);
            try
            {
                string streaming_manifest_json = await LoadTextFromStreamingAssetsAsync(streaming_manifest_path);
                var streaming_manifest_obj = JsonUtility.FromJson<BundleManifest>(streaming_manifest_json);
                mManifest_StreamingAssets = new XAssetBundleManifest(streaming_manifest_obj);
                streaming_manifest_obj = null;
            }
            catch (FileNotFoundException) { /* do nothing */ }
            catch (VFSException e)
            {
                mStartException = e;
                return false;
            }

            #endregion

            #region StreamingAssets assetbundle files hash .

            #endregion

            return true;
        }

        public Task OnServiceClose()
        {
            return Task.CompletedTask;
        }



        public T Load<T>(string assetPath) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public UnityEngine.Object Load(string assetPath, Type type)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }


        public VFSException GetStartException()
        {
            return mStartException;
        }

        public async void RunTest()
        {
            Debug.Log("关于在StreamingAssets中加载文件的测试");
            string file_path = Path.Combine(Application.streamingAssetsPath, "test.txt");
            byte[] bytes = await this.LoadFileFromStreamingAssetsAsync(file_path);
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("text:" + text);
        }


        private async UniTask<byte[]> LoadFileFromStreamingAssetsAsync(string path)
        {
            var req = UnityWebRequest.Get(path);
            var operation = req.SendWebRequest();
            var result = await operation;

            if (result.isHttpError)
            {
                if (result.responseCode == 404)
                    throw new Exceptions.FileNotFoundException($"Failed to load file from StreamingAssets, file path:{path}", path);
            }
            return result.downloadHandler.data;
        }

        private async UniTask<string> LoadTextFromStreamingAssetsAsync(string path, Encoding encoding = null)
        {
            try
            {
                byte[] bytes = await this.LoadFileFromStreamingAssetsAsync(path);
                return (encoding == null) ? Encoding.UTF8.GetString(bytes) : encoding.GetString(bytes);
            }
            catch (Exceptions.FileNotFoundException e404)
            {
                throw e404;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region VFS 加载流程

        private UniTask<IAsset> loadAssetAsync<T>(string assetPath) where T: UnityEngine.Object
        {
            if(QueryAsset(assetPath, out var result , out var group))
            {
                //可被加载
                //那就加载吧，首先加载Bundle
                return default;
            }
            else
            {
                throw new VFSException((IsChinese ? "被加载的asset的路径是无效的，它不在VFS的管理范围内" : "The asset path you want to load is valid. It is not under the management of VFS") + "Path:" + assetPath, VFSErrorCode.ValidLoadPath);
            }
        }

        #endregion 

        #region 加载AssetBundle_Async

        /// <summary>
        /// 加载AssetBundle和它的依赖， 异步总入口
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        private UniTask<AssetBundle> loadAssetBundleAndDependenciesAsync(string assetbundleName)
        {

            return default;
        }

        /// <summary>
        /// 加载一个AssetBundle，不考虑依赖， 异步总入口
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        private UniTask<AssetBundle> loadAssetBundleAsync(string assetBundle)
        {
            return default;
        }


        #endregion

        /// <summary>
        /// 查询资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns>is valid</returns>
        private bool QueryAsset(string path, out AssetQueryResult result, out VFSGroup matched_group)
        {
            result = new AssetQueryResult();
            result.AssetPath = path;
            result.AssetPathLower = path.ToLower();
            result.AssetExtensionName = XPath.GetExtension(result.AssetPathLower,true);

            //检查有没有被全局的规则拦下来
            if (this.IgnoreByGlobalConfig(result.AssetPathLower, result.AssetExtensionName))
            {
                result.Vliad = false;
                matched_group = null;
                return false;
            }

            //有效组查询
            foreach(var group in mGroups)
            {
                if (group.IsAssetPathMatch(path))
                {
                    result.Vliad = true;
                    result.GroupName = group.GroupName;
                    string assetbundle_name = group.GetAssetBundleNameOfAsset(path, out var buildType, out var devType); //获取到的assetbundle是不带后缀的
                    result.AssetBundleName = assetbundle_name + mConfig.AssetBundleFileExtension;
                    result.AssetBundleNameWithoutExtension = assetbundle_name;
                    result.DevelopType = devType;
                    result.BuildType = buildType;
                    matched_group = group;
                    return true;
                }
            }
            result.Vliad = false;
            matched_group = null;
            return false;
        }

        /// <summary>
        /// 是否被全局配置项所忽略
        /// </summary>
        /// <param name="path_lower">小写处理后的路径</param>
        /// <param name="extension">扩展名,传进来的是小写，以点开头的</param>
        /// <returns></returns>
        private bool IgnoreByGlobalConfig(string path_lower, string extension)
        {
            //后缀名忽略 //据说迭代器循环的效率比LINQ高，所以先用迭代器，有机会细测一下
            foreach(var item in mConfig.GlobalVFS_Ignore_ExtName) //在初始化过程中，配置中的数据都被规范化成小写，以点开头的格式，并且去掉了重复
            {
                if (item.Equals(extension)) return true;
            }

            //Path Item 
            if(mConfig.GlobalVFS_Ignore_Path_Item != null && mConfig.GlobalVFS_Ignore_Path_Item.Length > 0)
            {
                string[] path_items = path_lower.Split('/');
                foreach(var ignore_item in mConfig.GetGlobalVFS_Ignore_Path_Item(true, false)) //获取到的数据已经是小写，并且有缓存
                {
                    foreach(var path_item in path_items)
                    {
                        if (ignore_item.Equals(path_item)) return true;
                    }
                }
            }

            return false;
        }

        private bool IsAssetLoaded(string lower_path)
        {
            return false;   //TODO: 检查资源是否已加载
        }


        #region Customizable_Default_Function
        private string getWebAssetUrl(string platform_name, string assetBundleName, ref VFSGroup group, bool isExtensionGroup)
        {
            if (isExtensionGroup)
                return $"{platform_name}/{group.GroupName}/{assetBundleName}";
            else
                return $"{platform_name}/{assetBundleName}";
        }

        #endregion

    }

    /// <summary>
    /// Get download url of Web Assets
    /// </summary>
    /// <param name="platform_name"></param>
    /// <param name="assetBundleName"></param>
    /// <param name="group"></param>
    /// <param name="isExtensionGroup"></param>
    /// <returns></returns>
    public delegate string GetWebAssetUrlDelegate(string platform_name, string assetBundleName, ref VFSGroup group, bool isExtensionGroup);

}