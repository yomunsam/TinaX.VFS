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

        public bool Override_StreamingAssetsPath { get; private set; } = false;
        private string mVirtualDisk_MainPackageFolderPath;


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

        public VFSKit()
        {
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

        private 

        #endregion 

        #region 加载AssetBundle_Async




        #endregion


    }
}