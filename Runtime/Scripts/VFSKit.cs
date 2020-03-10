using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX;
using TinaX.Utils;
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
        public const string DefaultDownloadWebAssetUrl = "http://localhost:8080/";
        public string ConfigPath { get; set; } = VFSConst.ConfigFilePath_Resources;
        public AssetLoadType ConfigLoadType { get; private set; } = AssetLoadType.Resources;

        public XRuntimePlatform Platform { get; private set; }
        public string PlatformText { get; private set; }
        public string DownloadWebAssetUrl => webVfs_asset_download_base_url;
        public string VirtualDiskPath { get; private set; }

        public VFSCustomizable Customizable { get; private set; }

        public int DownloadWebAssetTimeout { get; set; } = 10;
        public int DownloadAssetBundleManifestTimeout { get; set; } = 5;

        public bool Override_StreamingAssetsPath { get; private set; } = false;

        private string mVirtualDisk_MainPackageFolderPath;
        private string mVirtualDisk_DataFolderPath;
        private string mVirtualDisk_ExtensionGroupRootFolderPath;

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
        /// StreamingAssets -> packages root
        /// </summary>
        private string mStreamingAssets_PackagesRootFolderPath; //因为这个地址可能会被重写，所以必须要从这里读取。
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
        private Dictionary<string, VFSGroup> mDict_Groups = new Dictionary<string, VFSGroup>();


        private VFSException mStartException;


        private string webVfs_asset_download_base_url;
        private bool webvfs_download_base_url_modify = false; //如果被profile或者手动修改过的话，这里为true

        /// <summary>
        /// 获取WebAssets的Url请调用这里的委托变量
        /// </summary>
        internal GetWebAssetDownloadUrlDelegate GetWebAssetUrl;
        /// <summary>
        /// 获取hash file 的url请调用这个（不包含开头的https://xxx/
        /// </summary>
        internal GetFileHashDownloadUrlDalegate GetWebFileHashBookUrl;
        /// <summary>
        /// 获取 assetbundleManifest的下载url （不包含开头的https://xxx/
        /// </summary>
        internal GetAssetBundleManifestDownloadUrlDalegate GetAssetBundleManifestDoanloadUrl;
        internal BundlesManager Bundles { get; private set; } = new BundlesManager();
        internal AssetsManager Assets { get; private set; } = new AssetsManager();
        internal ExtensionGroupsManager ExtensionGroups { get; private set; }

        private bool mInited = false;
        private bool mWebVFSReady = false;


        public VFSKit()
        {
            ExtensionGroups = new ExtensionGroupsManager(this); 
            Customizable = new VFSCustomizable(this);
            this.GetWebAssetUrl = default_getWebAssetUrl;
            this.GetWebFileHashBookUrl = default_getWebFilesHashUrl;
            this.GetAssetBundleManifestDoanloadUrl = default_getAssetBundleManifestDoanloadUrl;

            mStreamingAssets_PackagesRootFolderPath = VFSUtil.GetPackagesRootFolderInStreamingAssets();
            mStreamingAssets_MainPackageFolderPath = VFSUtil.GetMainPackageFolderInStreamingAssets();
            mStreamingAssets_DataRootFolderPath = VFSUtil.GetDataFolderInStreamingAssets();
            mStreamingAssets_ExtensionGroupRootFolderPath = VFSUtil.GetExtensionGroupRootFolderInStreamingAssets();

            Platform = XPlatformUtil.GetXRuntimePlatform(Application.platform);
            PlatformText = XPlatformUtil.GetNameText(Platform);

#if UNITY_EDITOR
            //load mode
            var loadMode = VFSLoadModeInEditor.GetLoadMode();
            switch (loadMode)
            {
                case RuntimeAssetsLoadModeInEditor.LoadByAssetDatabase:
                    mLoadByAssetDatabaseInEditor = true;
                    Debug.Log("[TinaX] VFS:" + (IsChinese ? $"<color=#{Internal.XEditorColorDefine.Color_Emphasize_16}>基于编辑器策略，采用编辑器方式加载资源</color>" : $"<color=#{Internal.XEditorColorDefine.Color_Emphasize_16}>Load assets by UnityEditor.AssetDatabase</color>"));
                    break;
                case RuntimeAssetsLoadModeInEditor.Normal:
                    mLoadByAssetDatabaseInEditor = false;
                    break;
                case RuntimeAssetsLoadModeInEditor.Override_StreamingAssetsPath:
                    Override_StreamingAssetsPath = true;
                    mStreamingAssets_PackagesRootFolderPath = VFSLoadModeInEditor.Get_Override_StreamingAssets_PackasgeRootFolderPath();
                    mStreamingAssets_MainPackageFolderPath = VFSUtil.GetMainPackageFolderInPackages(mStreamingAssets_PackagesRootFolderPath);
                    mStreamingAssets_DataRootFolderPath = VFSUtil.GetDataFolderInPackages(mStreamingAssets_PackagesRootFolderPath);
                    mStreamingAssets_ExtensionGroupRootFolderPath = VFSUtil.GetExtensionGroupRootFolderInPackages(mStreamingAssets_PackagesRootFolderPath);
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
            Debug.Log("启动VFS");
            if (mInited) return true;
            #region Configs
            // load config by xconfig | VFS not ready, so vfs config can not load by vfs.
            var config = XConfig.GetConfig<VFSConfigModel>(ConfigPath);
            try
            {
                UseConfig(config);
            }
            catch (VFSException e)
            {
                mStartException = e;
                return false;
            }


            #endregion

            //init vfs virtual disk folder
            VirtualDiskPath = Path.Combine(Application.persistentDataPath, "VFS_VDisk"); //TODO: 在Windows之类目录权限比较自由的平台，未来可以考虑搞个把这个目录移动到别的地方的功能。（毕竟有人不喜欢把太多文件扔在C盘）
            XDirectory.CreateIfNotExists(VirtualDiskPath);
            mVirtualDisk_MainPackageFolderPath = VFSUtil.GetMainPackageFolderInPackages(VirtualDiskPath);
            mVirtualDisk_DataFolderPath = VFSUtil.GetDataFolderInPackages(VirtualDiskPath);
            mVirtualDisk_ExtensionGroupRootFolderPath = VFSUtil.GetExtensionGroupRootFolderInPackages(VirtualDiskPath);

            #region Manifest and FilesHash ...
            try
            {
                List<Task> list_init_manifest_and_hashs_tasks = new List<Task>();
                if (mGroups != null && mGroups.Count > 0)
                {
                    foreach (var group in mGroups)
                    {
                        var task_manifest = InitGroupManifests(group);
                        list_init_manifest_and_hashs_tasks.Add(task_manifest);
                        var task_hash = InitGroupFilesHash(group);
                        list_init_manifest_and_hashs_tasks.Add(task_hash);
                    }
                }
                await Task.WhenAll(list_init_manifest_and_hashs_tasks);
            }
            catch(VFSException e)
            {
                mStartException = e;
#if UNITY_EDITOR
                Debug.LogError(e);
#endif
                return false;
            }
            
            #endregion



            bool need_init_webvfs = false;
            if (mConfig.InitWebVFSOnStart)
            {
                need_init_webvfs = true;
#if UNITY_EDITOR
                if (mLoadByAssetDatabaseInEditor) need_init_webvfs = false;
#endif
            }
            if (need_init_webvfs)
            {
                try
                {
                    await InitWebVFS();
                }
                catch (VFSException e)
                {
                    mStartException = e;
#if UNITY_EDITOR
                    Debug.LogError(e.Message);
#endif
                    return false;
                }
            }

            mInited = true;
            return true;
        }

        public async Task InitWebVFS()
        {
            #region WebVFS Url
            var web_vfs_config = XConfig.GetConfig<WebVFSNetworkConfig>(VFSConst.Config_WebVFS_URLs);
            if (web_vfs_config != null)
            {
                await this.UseWebVFSNetworkConfig(web_vfs_config);
            }
            #endregion

            #region WebVFS FileHash
            foreach(var group in mGroups)
            {
                //TODO
            }
            #endregion


            mWebVFSReady = true;
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

        public async Task<T> LoadAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            var asset = await this.LoadAssetAsync<T>(assetPath);
            return asset.Get<T>();
        }

        public void LoadAsync<T>(string assetPath,Action<T> callback) where T : UnityEngine.Object
        {
            this.LoadAsync<T>(assetPath)
                .ToObservable<T>()
                .SubscribeOnMainThread()
                .Subscribe(t =>
                {
                    callback?.Invoke(t);
                });
        }

        public void LoadAsync<T>(string assetPath, Action<T,VFSException> callback) where T : UnityEngine.Object
        {
            this.LoadAsync<T>(assetPath)
                .ToObservable<T>()
                .SubscribeOnMainThread()
                .Subscribe(t =>
                {
                    callback?.Invoke(t,null);
                },
                e=>
                {
                    if (e is VFSException)
                        callback?.Invoke(null, e as VFSException);
                    else
                        throw e;
                });
        }

        public async Task<IAsset> LoadAssetAsync<T> (string assetPath) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor)
            {
                //要查询的
                if (this.QueryAsset(assetPath, out var result, out var group))
                {
                    return await loadAssetFromAssetDatabase<T>(assetPath);
                }
                else
                {
                    throw new VFSException((IsChinese ? "被加载的asset的路径是无效的，它不在VFS的管理范围内" : "The asset path you want to load is valid. It is not under the management of VFS")
                                           + "Path:"
                                           + assetPath, VFSErrorCode.ValidLoadPath);
                }
            }
#endif
            return await this.loadAssetAsync<T>(assetPath);
        }

        public async Task<IAsset> LoadAssetAsync(string assetPath, Type type)
        {
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor)
            {
                //要查询的
                if (this.QueryAsset(assetPath, out var result, out var group))
                {
                    return await loadAssetFromAssetDatabase(assetPath,type);
                }
                else
                {
                    throw new VFSException((IsChinese ? "被加载的asset的路径是无效的，它不在VFS的管理范围内" : "The asset path you want to load is valid. It is not under the management of VFS")
                                           + "Path:"
                                           + assetPath, VFSErrorCode.ValidLoadPath);
                }
            }
#endif
            return await this.loadAssetAsync(assetPath, type);
        }

        public void LoadAssetAsync<T>(string assetPath, Action<IAsset> callback) where T : UnityEngine.Object
        {
            this.LoadAssetAsync<T>(assetPath)
                .ToObservable()
                .SubscribeOnMainThread()
                .Subscribe(asset =>
                {
                    callback?.Invoke(asset);
                });
        }

        public void LoadAssetAsync<T>(string assetPath, Action<IAsset,VFSException> callback) where T : UnityEngine.Object
        {
            this.LoadAssetAsync<T>(assetPath)
                .ToObservable()
                .SubscribeOnMainThread()
                .Subscribe(asset =>
                {
                    callback?.Invoke(asset, null);
                }, e =>
                {
                    if (e is VFSException)
                    {
                        callback?.Invoke(null, (VFSException)e);
                    }
                    else
                        throw e;
                });
        }

        public void Release(UnityEngine.Object asset)
        {
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) return;
#endif
            if(this.Assets.TryGetAsset(asset.GetHashCode(),out var vfs_asset))
                vfs_asset.Release();

        }

        public void UnloadUnusedAssets()
        {
            this.Assets.Refresh();
            this.Bundles.Refresh();
        }

        public VFSException GetStartException()
        {
            return mStartException;
        }




        public VFSGroup[] GetAllGroups()
        {
            return mGroups.ToArray();
        }

        /// <summary>
        /// 使用配置，如果有效，返回true
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public void UseConfig(VFSConfigModel config)
        {
            if (config == null)
            {
                throw new VFSException("Load VFS config failed, \nload type:" + ConfigLoadType.ToString() + "\nload path:" + ConfigPath, VFSErrorCode.LoadConfigFailed);
            }
            VFSUtil.NormalizationConfig(ref config);

            if (!VFSUtil.CheckConfiguration(ref config, out var errorCode, out var folderError))
            {
                throw new VFSException("VFS Config Error:", errorCode);
            }
            mConfig = config;

            // init configs data.
            mGroups.Clear();
            mDict_Groups.Clear();
            if (mConfig.Groups != null)
            {
                foreach (var groupOption in mConfig.Groups)
                {
                    if (!groupOption.ExtensionGroup)
                    {
                        var group = new VFSGroup(groupOption);
                        mGroups.Add(group);

                        mDict_Groups.Add(group.GroupName, group);
                    }
                    //init each group status.
                }
            }

            if (!webvfs_download_base_url_modify)
            {
                webVfs_asset_download_base_url = mConfig.DefaultWebVFSBaseUrl;
            }
        }

        public void SetDownloadWebAssetUrl(string url)
        {
            webvfs_download_base_url_modify = true;
            webVfs_asset_download_base_url = url;
            if (!webVfs_asset_download_base_url.EndsWith("/"))
            {
                webVfs_asset_download_base_url += "/";
            }
        }

        public async Task UseWebVFSNetworkConfig(WebVFSNetworkConfig config)
        {
            bool flag = false;
            if (config != null || config.Configs != null && config.Configs.Length > 0)
            {
                //尝试获取profile
                string profile = XCore.GetMainInstance().ProfileName;
                bool developMode = XCore.GetMainInstance().DevelopMode;
                var results = config.Configs.Where(item => item.ProfileName == profile);
                if(results.Count() > 0)
                {
                    var result = results.First();
                    //挨个寻找合适的服务器
                    if(result.Urls!= null && result.Urls.Length > 0)
                    {
                        foreach(var item in result.Urls)
                        {
                            if (item.NetworkMode == WebVFSNetworkConfig.NetworkMode.DevelopMode && !developMode)
                                continue;
                            if (item.NetworkMode == WebVFSNetworkConfig.NetworkMode.Editor && !Application.isEditor)
                                continue;

                            bool b = await SayHelloToWebServer(item.HelloUrl, 4);
                            if (b)
                            {
                                flag = true;
                                webVfs_asset_download_base_url = item.BaseUrl;
                                break;
                            }
                        }
                    }
                }
            }


            if (!flag)
            {
                //执行到这里，没有在prefile配置中找到有效的url的话，就用mconfig里的
                webVfs_asset_download_base_url = mConfig.DefaultWebVFSBaseUrl;
            }

            if(webVfs_asset_download_base_url.IsNullOrEmpty() || webVfs_asset_download_base_url.IsNullOrWhiteSpace())
                webVfs_asset_download_base_url = DefaultDownloadWebAssetUrl;

            if (!webVfs_asset_download_base_url.EndsWith("/"))
                webVfs_asset_download_base_url += "/";

            webvfs_download_base_url_modify = true;
        }

        private async UniTask<byte[]> LoadFileFromStreamingAssetsAsync(string path)
        {
            var req = UnityWebRequest.Get(path);
            await req.SendWebRequest();
            if (req.isHttpError)
            {
                if (req.responseCode == 404)
                    throw new Exceptions.FileNotFoundException($"Failed to load file from StreamingAssets, file path:{path}", path);
            }
            return req.downloadHandler.data;
        }

        private async UniTask<string> LoadTextFromStreamingAssetsAsync(string path)
        {
            var req = UnityWebRequest.Get(path);
            await req.SendWebRequest();
            if (req.isHttpError)
            {
                if (req.responseCode == 404)
                    throw new Exceptions.FileNotFoundException($"Failed to load file from StreamingAssets, file path:{path}", path);
            }
            return VFSUtil.RemoveInvalidHead(req.downloadHandler.text);
        }


        private  async UniTask<string> LoadTextFromWebAsync(Uri uri, int timeout = 3, Encoding encoding = null)
        {
            Debug.Log("喵，下载文本：" + uri.ToString());
            var req = UnityWebRequest.Get(uri);
            req.timeout = timeout;
            await req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
            {
                if (req.responseCode == 404)
                    throw new FileNotFoundException("Failed to get text from web : " + uri.ToString(), uri.ToString());
                else
                    throw new VFSException("Failed to get text from web:" + uri.ToString());
            }
            if (encoding == null)
                return VFSUtil.RemoveInvalidHead(req.downloadHandler.text);
            else
                return VFSUtil.RemoveInvalidHead(encoding.GetString(req.downloadHandler.data));

        }

        private async Task InitGroupManifests(VFSGroup group)
        {
            bool init_streamingassets = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_streamingassets = false;
#endif
            //streamingassets assetbundleManifest
            if (init_streamingassets)
            {
                string stream_manifest_path = group.GetManifestFilePath(mStreamingAssets_PackagesRootFolderPath);
                try
                {
                    string stream_json = await LoadTextFromStreamingAssetsAsync(stream_manifest_path);
                    var bundleManifest = JsonUtility.FromJson<BundleManifest>(stream_json);
                    group.Manifest_StreamingAssets = new XAssetBundleManifest(bundleManifest);
                }
                catch (FileNotFoundException) { /* do nothing */ }
            }

            bool init_vdisk = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_vdisk = false;
#endif
            //vdisk manifest
            if (init_vdisk)
            {
                string vdisk_manifest_path = group.GetManifestFilePath(VirtualDiskPath);
                if (File.Exists(vdisk_manifest_path))
                {
                    try
                    {
                        string json = File.ReadAllText(vdisk_manifest_path);
                        var bundleManifest = JsonUtility.FromJson<BundleManifest>(json);
                        group.Manifest_VirtualDisk = new XAssetBundleManifest(bundleManifest);
                    }
                    catch { /* do nothing */ }
                }
            }

            //remote?
            bool init_remote = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_remote = false;
#endif
            if (group.HandleMode == GroupHandleMode.LocalAndUpdatable || group.HandleMode == GroupHandleMode.LocalOnly) init_remote = false;
            if (mWebVFSReady && init_remote)
            {

                string uri = this.GetWebAssetBundleManifestDoanloadUrl(this.PlatformText, group.ExtensionGroup, group.GroupName);
                try
                {
                    string json = await LoadTextFromWebAsync(new Uri(uri), this.DownloadAssetBundleManifestTimeout);
                    var bundleManifest = JsonUtility.FromJson<BundleManifest>(json);
                    group.Manifest_Remote = new XAssetBundleManifest(bundleManifest); 
                }
                catch (FileNotFoundException) { /* do nothing */ }
            }
        }
        private async Task InitGroupFilesHash(VFSGroup group)
        {
            //streamingassets assetbundleManifest
            bool init_streamingassets = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_streamingassets = false;
#endif
            if (init_streamingassets)
            {
                string hash_stream_path = group.GetAssetBundleHashsFilePath(mStreamingAssets_PackagesRootFolderPath);
                try
                {
                    string json = await LoadTextFromStreamingAssetsAsync(hash_stream_path);
                    group.FilesHash_StreamingAssets = JsonUtility.FromJson<FilesHashBook>(json);
                }
                catch (FileNotFoundException) { /* do nothing */ }
            }


            //vdisk manifest
            bool init_vdisk = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_vdisk = false;
#endif
            if (init_vdisk)
            {
                string vdisk_hash_path = group.GetAssetBundleHashsFilePath(VirtualDiskPath);
                if (File.Exists(vdisk_hash_path))
                {
                    try
                    {
                        string json = File.ReadAllText(vdisk_hash_path);
                        group.FilesHash_VirtualDisk = JsonUtility.FromJson<FilesHashBook>(json);
                    }
                    catch { /* do nothing */ }
                }
            }


            //remote?
            bool init_remote = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_remote = false;
#endif
            if (group.HandleMode == GroupHandleMode.LocalAndUpdatable || group.HandleMode == GroupHandleMode.LocalOnly) init_remote = false;
            if (mWebVFSReady && init_remote)
            {
                string uri = this.GetWebHashsFileDownloadUrl(this.PlatformText, group.ExtensionGroup, group.GroupName);
                try
                {
                    string json = await LoadTextFromWebAsync(new Uri(uri), this.DownloadAssetBundleManifestTimeout);
                    group.FilesHash_Remote = JsonUtility.FromJson<FilesHashBook>(json);
                }
                catch (FileNotFoundException) { /* do nothing */ }
            }
        }

        #region VFS Asset 异步加载

        private async UniTask<IAsset> loadAssetAsync<T>(string assetPath) where T: UnityEngine.Object
        {
            if(QueryAsset(assetPath, out var result , out var group))
            {
                VFSAsset asset;
                //是否已加载
                lock (this)
                {
                    bool load_flag = this.IsAssetLoadedOrLoading(result.AssetPathLower, out asset);
                    if (!load_flag)
                    {
                        asset = new VFSAsset(group, result);
                        asset.LoadTask = doLoadAssetAsync<T>(asset);
                        this.Assets.Register(asset);
                    }
                    else
                        asset.Retain();
                }

                if(asset.LoadState != AssetLoadState.Loaded)
                {
                    await asset.LoadTask;
                }
                return asset;
            }
            else
            {
                throw new VFSException((IsChinese ? "被加载的asset的路径是无效的，它不在VFS的管理范围内" : "The asset path you want to load is valid. It is not under the management of VFS") + "Path:" + assetPath, VFSErrorCode.ValidLoadPath);
            }
        }

        private async UniTask<IAsset> loadAssetAsync(string assetPath, Type type)
        {
            if (QueryAsset(assetPath, out var result, out var group))
            {
                VFSAsset asset;
                //是否已加载
                lock (this)
                {
                    bool load_flag = this.IsAssetLoadedOrLoading(result.AssetPathLower, out asset);
                    if (!load_flag)
                    {
                        asset = new VFSAsset(group, result);
                        asset.LoadTask = doLoadAssetAsync(asset,type);
                        this.Assets.Register(asset);
                    }
                    else
                        asset.Retain();
                }

                if (asset.LoadState != AssetLoadState.Loaded)
                {
                    await asset.LoadTask;
                }
                return asset;
            }
            else
            {
                throw new VFSException((IsChinese ? "被加载的asset的路径是无效的，它不在VFS的管理范围内" : "The asset path you want to load is valid. It is not under the management of VFS") + "Path:" + assetPath, VFSErrorCode.ValidLoadPath);
            }
        }

        //只执行加载，不做判断，啥也不干
        private async UniTask doLoadAssetAsync<T>(VFSAsset asset)
        {
            if (asset.LoadState != AssetLoadState.Loaded && asset.LoadState != AssetLoadState.Unloaded) asset.LoadState = AssetLoadState.Loading;
            if(asset.Bundle == null)
            {
                //来加载bundle吧
                asset.Bundle = await loadAssetBundleAndDependenciesAsync(asset.QueryResult.AssetBundleName, asset.Group,true);
            }
            await asset.LoadAsync<T>();
            this.Assets.RegisterHashCode(asset);
        }

        private async UniTask doLoadAssetAsync(VFSAsset asset, Type type)
        {
            if (asset.LoadState != AssetLoadState.Loaded && asset.LoadState != AssetLoadState.Unloaded) asset.LoadState = AssetLoadState.Loading;
            if (asset.Bundle == null)
            {
                //来加载bundle吧
                asset.Bundle = await loadAssetBundleAndDependenciesAsync(asset.QueryResult.AssetBundleName, asset.Group, true);
            }
            await asset.LoadAsync(type);
            this.Assets.RegisterHashCode(asset);
        }
        #endregion 

        #region 加载AssetBundle_Async

        /// <summary>
        /// 加载AssetBundle和它的依赖， 异步入口
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <param name="counter">引用计数器</param>
        /// <param name="load_chain">加载链：如果是从外部调用的加载，这里为空，如果是递归，则把递归过程中的每一项都加入到加载链</param>
        /// <returns></returns>
        private async UniTask<VFSBundle> loadAssetBundleAndDependenciesAsync(string assetbundleName, VFSGroup group, bool counter = true, List<string> load_chain = null)
        {
            VFSBundle bundle;
            //是否已经加载了
            lock (this)
            {
                bool load_flag = this.Bundles.TryGetBundle(assetbundleName, out bundle);
                if (!load_flag)
                {
                    bundle = new VFSBundle();
                    bundle.AssetBundleName = assetbundleName;
                    bundle.LoadTask = doLoadAssetBundleAndDependenciesAsync(bundle, group, load_chain);
                    this.Bundles.Register(bundle);
                }
            }

            if (bundle.LoadState != AssetLoadState.Loaded)
                await bundle.LoadTask;

            if (counter)
                bundle.Retain();
            return bundle;
        }

        private async UniTask doLoadAssetBundleAndDependenciesAsync(VFSBundle bundle, VFSGroup group, List<string> load_chain = null)
        {
            //bool dep_self_tag = false; //自依赖 标记 //暂时好像不需要它，给注释掉看看
            List<string> dep_self_list = new List<string>(); //自依赖 列表
            
            //加载链
            if (load_chain == null) load_chain = new List<string>();
            if (!load_chain.Contains(bundle.AssetBundleName)) //这种情况不应该出现，如果有重复加载项应该会被拦在外面一层方法里
                load_chain.Add(bundle.AssetBundleName);
            //依赖
            string[] dependencies;
            this.TryGetDirectDependencies(bundle.AssetBundleName, out dependencies,out _, group);

            bundle.DependenciesNames = dependencies;

            List<UniTask<VFSBundle>> list_dep_load_task = new List<UniTask<VFSBundle>>(); //需要等待的依赖列表
            //加载依赖
            if(dependencies != null && dependencies.Length > 0)
            {
                foreach(var d in dependencies)
                {
                    if (this.TryGetDirectDependencies(d, out var ds, out var dg, group)) 
                    {
                        //自依赖判定
                        bool _dep_self = false;
                        if (load_chain.Contains(d))
                            _dep_self = true;

                        //加载依赖
                        
                        if (!_dep_self)
                        {
                            var task = this.loadAssetBundleAndDependenciesAsync(d, dg, false, load_chain);
                            list_dep_load_task.Add(task);
                        }
                    }
                }

                if(list_dep_load_task.Count > 0)
                {
                    await UniTask.WhenAll(list_dep_load_task);
                }

                //把依赖对象装入本体
                foreach (var d in dependencies)
                {
                    if(this.Bundles.TryGetBundle(d, out var d_bundle)) //task那个列表不全（可能有自依赖），从那边获取好像更麻烦
                    {
                        if (!bundle.Dependencies.Contains(d_bundle))
                            bundle.Dependencies.Add(d_bundle);
                    }
                }
            }

            //加载本体
            bundle.LoadedPath = this.GetAssetBundleLoadPath(bundle.AssetBundleName,ref group,out string vdisk_path);
            bundle.VirtualDiskPath = vdisk_path;
            bundle.ABLoader = group.ABLoader;
            bundle.GroupHandleMode = group.HandleMode;
            bundle.DownloadTimeout = this.DownloadWebAssetTimeout;

            await bundle.LoadAsync();
        }

        #endregion

        #region VFS Asset 同步加载

        #endregion

        #region 同步加载AssetBundle
        private VFSBundle loadAssetBundleAndDependencies(string assetbundleName, VFSGroup group, bool counter = true, List<string> load_chain = null)
        {
            VFSBundle bundle;
            //是否已经加载了
            if (this.Bundles.TryGetBundle(assetbundleName, out bundle))
            {
                lock (this)
                {
                    if(bundle.LoadState == AssetLoadState.Loaded)
                    {
                        if (counter)
                            bundle.Retain();
                        return bundle;
                    }
                }
            }
            //再次检查有没有能用的sync的temp ab
            if(this.Bundles.TryGetBundleSync(assetbundleName,out bundle))
            {
                if(bundle.LoadState == AssetLoadState.Loaded)
                {
                    if (counter)
                        bundle.Retain();
                    return bundle;
                }
            }
            /*
             * 同步加载和Task的异步逻辑之间是冲突的：
             *      如果有一个异步正在loading中的Bundle,在同步加载的时候是没用的，同步线程没法去等待它。
             * 所以在同步加载这边，检查到没有已经被加载完成的Bundle的话，直接开始在主线程加载新的bundle，
             * 等加载完了再去看cache里有没有一样的bundle,如果没有，register进去，如果有，丢弃同步加载出来的bundle。
             * 
             */

            bundle = new VFSBundle();
            bundle.AssetBundleName = assetbundleName;

            //加载链
            if (load_chain == null) load_chain = new List<string>();
            if (!load_chain.Contains(bundle.AssetBundleName)) 
                load_chain.Add(bundle.AssetBundleName);

            //依赖
            string[] dependencies;
            this.TryGetDirectDependencies(bundle.AssetBundleName, out dependencies, out _, group);
            bundle.DependenciesNames = dependencies;

            //加载依赖
            if(dependencies != null && dependencies.Length > 0)
            {
                foreach(var d in dependencies)
                {
                    if(this.TryGetDirectDependencies(d,out var ds, out var dg, group))
                    {
                        //加载依赖

                        //自依赖判定
                        bool dep_self = false;
                        if (load_chain.Contains(d))
                            dep_self = true;

                        if (!dep_self)
                        {
                            //加载
                            loadAssetBundleAndDependencies(d, dg, false, load_chain);
                        }

                    }
                }

                //把依赖对象装入本体
                foreach(var d in dependencies)
                {
                    if(this.Bundles.TryGetBundle(d,out var d_bundle))
                    {
                        if (!bundle.Dependencies.Contains(d_bundle))
                            bundle.Dependencies.Add(d_bundle);
                    }
                }
            }

            //加载本体
            bundle.LoadedPath = this.GetAssetBundleLoadPathWithoutRemote(bundle.AssetBundleName, ref group, out string vdisk_path);
            bundle.VirtualDiskPath = vdisk_path;
            bundle.ABLoader = group.ABLoader;
            bundle.GroupHandleMode = group.HandleMode;
            bundle.DownloadTimeout = this.DownloadWebAssetTimeout; //其实没啥用

            bundle.Load();

            //同步加载结束，再次检查cache
            lock (this)
            {
                if(this.Bundles.TryGetBundle(bundle.AssetBundleName,out var _bundle))
                {
                    //有bundle
                    if(_bundle.LoadState == AssetLoadState.Loaded)
                    {
                        bundle.Dependencies.Clear();
                        bundle.DependenciesNames = null;
                        bundle.Unload();
                        bundle = null;
                        if (counter)
                            _bundle.Retain();
                        return _bundle;
                    }
                    else
                    {
                        //有一个还没加载完的
                        this.Bundles.RegisterSyncTemp(bundle);
                        if (counter)
                            bundle.Retain();
                        return bundle;
                    }
                }
                else
                {
                    //没有，把自己加进去
                    this.Bundles.Register(bundle);
                    if (counter)
                        bundle.Retain();
                    return bundle;
                }
            }

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

            VFSGroup myGroup = null;
            //有效组查询
            foreach(var group in mGroups)
            {
                if (group.IsAssetPathMatch(path))
                {
                    myGroup = group;
                    break;
                }
            }
            if(myGroup == null)
            {
                foreach(var group in this.ExtensionGroups.mGroups)
                {
                    if (group.IsAssetPathMatch(path))
                    {
                        myGroup = group;
                        break;
                    }
                }
            }
            if(myGroup != null)
            {
                result.Vliad = true;
                result.GroupName = myGroup.GroupName;
                string assetbundle_name = myGroup.GetAssetBundleNameOfAsset(path, out var buildType, out var devType); //获取到的assetbundle是不带后缀的
                result.AssetBundleName = assetbundle_name + mConfig.AssetBundleFileExtension;
                result.AssetBundleNameWithoutExtension = assetbundle_name;
                result.DevelopType = devType;
                result.BuildType = buildType;
                result.ExtensionGroup = myGroup.ExtensionGroup;
                result.GroupHandleMode = myGroup.HandleMode;
                matched_group = myGroup;
                return true;
            }
            result.Vliad = false;
            matched_group = null;
            return false;
        }

        /// <summary>
        /// 尝试获取直接依赖
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="dependencies"></param>
        /// <param name="group">被查询依赖的AssetBundle所在的组</param>
        /// <param name="priority">优先查询组：如果不为空，将优先在传入的组中查询</param>
        /// <returns></returns>
        private bool TryGetDirectDependencies(string assetBundleName,out string[] dependencies, out VFSGroup group, VFSGroup priority = null)
        {
            if(priority != null)
            {
                if(priority.AssetBundleManifest.TryGetDirectDependencies(assetBundleName, out dependencies))
                {
                    group = priority;
                    return true;
                }
            }
            foreach(var g in this.mGroups)
            {
                if (priority != null && g == priority) continue;
                if (g.AssetBundleManifest.TryGetDirectDependencies(assetBundleName, out dependencies))
                {
                    group = g;
                    return true;
                }
            }
            
            foreach (var g in this.ExtensionGroups.mGroups)
            {
                if (priority != null && g == priority) continue;
                if (g.AssetBundleManifest.TryGetDirectDependencies(assetBundleName, out dependencies))
                {
                    group = g;
                    return true;
                }
            }
            dependencies = Array.Empty<string>();
            group = null;
            return false;
        }

        public bool TryGetGroup(string groupName, out VFSGroup group)
        {
            if(this.mDict_Groups.TryGetValue(groupName,out group))
            {
                return true;
            }
            else
            {
                if(this.ExtensionGroups.TryGetExtensionGroup(groupName, out var ex_group))
                {
                    group = ex_group;
                    return true;
                }
                else
                {
                    return false;
                }
            }
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

        private bool IsAssetLoadedOrLoading(string lower_path, out VFSAsset asset)
        {
            return this.Assets.TryGetAsset(lower_path, out asset);
        }

        /// <summary>
        /// AssetBundle 正在加载中或已加载
        /// </summary>
        /// <param name="bundle_name"></param>
        /// <param name="bundle"></param>
        /// <returns></returns>
        private bool IsBundleLoadedOrLoading(string bundle_name,out VFSBundle bundle)
        {
            return Bundles.TryGetBundle(bundle_name, out bundle);
        }

        /// <summary>
        /// 获取资源的加载路径
        /// </summary>
        /// <param name="assetbundle"></param>
        /// <param name="result"></param>
        /// <param name="vdisk_path">这个资源如果保存在vidsk的话，它的路径应该是啥</param>
        /// <returns></returns>
        private string GetAssetBundleLoadPath(string assetbundle, ref VFSGroup group,out string vdisk_path)
        {
            if (group == null) { vdisk_path = string.Empty; return string.Empty; }
            //没有匹配的组，无效

            if(group.HandleMode == GroupHandleMode.RemoteOnly)
            {
                //资源只有可能在web,
                vdisk_path = string.Empty;
                return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
            }

            //检查资源是否在Virtual Disk
            string asset_path_vdisk = group.ExtensionGroup ? VFSUtil.GetAssetPath(true, mVirtualDisk_ExtensionGroupRootFolderPath, assetbundle, group.GroupName) : VFSUtil.GetAssetPath(false, mVirtualDisk_MainPackageFolderPath, assetbundle);
            vdisk_path = asset_path_vdisk;

            if (File.Exists(asset_path_vdisk))
            {
                //资源存在，检查：如果这个资源是LocalOrRemote，并且本地hash与云端不一致的话，则使用云端地址
                if(group.HandleMode == GroupHandleMode.LocalOrRemote && mWebVFSReady)
                {
                    string hash_vdisk = XFile.GetMD5(asset_path_vdisk);
                    string hash_streaming = string.Empty;
                    if(group.FilesHash_StreamingAssets != null)
                        group.FilesHash_StreamingAssets.TryGetFileHashValue(assetbundle, out hash_streaming);

                    //尝试找到它在remote的hash
                    if (group.FilesHash_Remote != null)
                    {
                        if(group.FilesHash_Remote.TryGetFileHashValue(assetbundle, out var remote_hash))
                        {
                            if (!hash_vdisk.Equals(remote_hash))
                            {
                                //和vdisk的hash不一致了，检查下stream的，如果也不一致就用云端的了
                                if (!hash_streaming.IsNullOrEmpty())
                                {
                                    if (!hash_streaming.Equals(remote_hash))
                                    {
                                        //不一致，用remote吧
                                        this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                                    }
                                    else
                                    {
                                        //用steram的
                                        return VFSUtil.GetAssetPath(true, mStreamingAssets_ExtensionGroupRootFolderPath, assetbundle, group.GroupName);
                                    }
                                }
                                else
                                {
                                    //在stream没有有效的hash，只能用remote的
                                    return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                                }
                            }
                        }
                    }
                    
                }

                //在上面没有被return 的话，返回vdisk的地址
                return asset_path_vdisk;
            }

            //已知文件不在Virtual Disk
            string asset_path_streamingassets = group.ExtensionGroup ? VFSUtil.GetAssetPath(true, mStreamingAssets_ExtensionGroupRootFolderPath, assetbundle, group.GroupName) : VFSUtil.GetAssetPath(false, mStreamingAssets_MainPackageFolderPath, assetbundle);

            //有没有可能这个文件在web?
            if (group.HandleMode == GroupHandleMode.LocalOrRemote && mWebVFSReady)
            {
                if (group.FilesHash_StreamingAssets == null)
                {
                    if (group.FilesHash_Remote != null)
                        return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group); //StreamingAssets找不到相关的信息，然后这个文件又有可能在remote,所以就直接认为它在remote了
                    else
                        return asset_path_streamingassets; //放弃
                }
                if (group.FilesHash_StreamingAssets.TryGetFileHashValue(assetbundle,out var stream_hash))
                {
                    if(group.FilesHash_Remote != null)
                    {
                        if(group.FilesHash_Remote.TryGetFileHashValue(assetbundle,out var remote_hash))
                        {
                            if (!remote_hash.Equals(stream_hash))
                            {
                                //不一致，用云端的
                                return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                            }
                        }
                    }
                }
                else
                {
                    //本地没有，用云端的
                    return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                }
            }
            
            return asset_path_streamingassets;
        }

        /// <summary>
        /// 取AssetBundle加载地址，不包含Web地址
        /// </summary>
        /// <param name="assetbundle"></param>
        /// <param name="vdisk_path"></param>
        /// <returns></returns>
        private string GetAssetBundleLoadPathWithoutRemote(string assetbundle,ref VFSGroup group, out string vdisk_path)
        {
            //检查资源是否在Virtual Disk
            if (group == null) { vdisk_path = string.Empty; return string.Empty; }
            if(group.HandleMode == GroupHandleMode.RemoteOnly) { vdisk_path = string.Empty; return string.Empty; }
            
            vdisk_path = group.ExtensionGroup ? VFSUtil.GetAssetPath(true, mVirtualDisk_ExtensionGroupRootFolderPath, assetbundle, group.GroupName) : VFSUtil.GetAssetPath(false, mVirtualDisk_MainPackageFolderPath, assetbundle);
            if (File.Exists(vdisk_path)) return vdisk_path;

            return group.ExtensionGroup ? VFSUtil.GetAssetPath(true, mStreamingAssets_ExtensionGroupRootFolderPath, assetbundle, group.GroupName) : VFSUtil.GetAssetPath(false, mStreamingAssets_MainPackageFolderPath, assetbundle);
        }

        /// <summary>
        /// 获取到完整的，拼接后的，以http://之类的开头的可以直接用的url
        /// </summary>
        /// <param name="platform_name"></param>
        /// <param name="assetBundleName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private string GetWebAssetDownloadUrl(string platform_name,string assetBundleName,ref VFSGroup group)
        {
            return this.DownloadWebAssetUrl + this.GetWebAssetUrl(platform_name, assetBundleName, ref group, group.ExtensionGroup);
        }

        /// <summary>
        /// 最终调用 获取完整的Url 下载assetbundle 文件hash
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="isExtensionGroup"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private string GetWebHashsFileDownloadUrl(string platform, bool isExtensionGroup, string groupName)
        {
            return this.DownloadWebAssetUrl + this.GetWebFileHashBookUrl(platform, isExtensionGroup, groupName);
        }

        /// <summary>
        /// 最终调用 获取完整的Url 下载assetbundle manifest
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="isExtensionGroup"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private string GetWebAssetBundleManifestDoanloadUrl(string platform, bool isExtensionGroup, string groupName)
        {
            return this.DownloadWebAssetUrl + this.GetAssetBundleManifestDoanloadUrl(platform, isExtensionGroup, groupName);
        }

        private async UniTask<bool> SayHelloToWebServer(string url, int timeout = 10)
        {
            Debug.Log("喵，say hello:" + url);
            var req = UnityWebRequest.Get(url);
            req.timeout = timeout;
            await req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
                return false;

            return (VFSUtil.RemoveInvalidHead(req.downloadHandler.text) == "hello");
        }

        #region Customizable_Default_Function
        private string default_getWebAssetUrl(string platform_name, string assetBundleName, ref VFSGroup group, bool isExtensionGroup)
        {
            if (isExtensionGroup)
                return $"{platform_name}/ext/{group.GroupName}/{assetBundleName}";
            else
                return $"{platform_name}/main/{assetBundleName}";
        }

        private string default_getWebFilesHashUrl(string platform_name, bool isExtensionGroup, string groupName)
        {
            if (isExtensionGroup)
                return $"{platform_name}/ext/{groupName}/{VFSConst.AssetBundleFilesHash_FileName}";
            else
                return $"{platform_name}/main_data/{VFSConst.MainPackage_AssetBundle_Hash_Files_Folder}/{groupName.GetMD5(true, true)}.json";
        }

        private string default_getAssetBundleManifestDoanloadUrl(string platform_name, bool isExtensionGroup, string groupName)
        {
            if (isExtensionGroup)
                return $"{platform_name}/ext/{groupName}/{VFSConst.AssetBundleManifestFileName}";
            else
                return $"{platform_name}/main_data/{VFSConst.MainPackage_AssetBundleManifests_Folder}/{groupName.GetMD5(true, true)}.json";

        }


        #endregion


#if UNITY_EDITOR
        #region 编辑器下的AssetDatabase加载
        private async Task<IAsset> loadAssetFromAssetDatabase<T>(string asset_path) where T : UnityEngine.Object
        {
            await UniTask.DelayFrame(1);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(asset_path);
            return new EditorAsset(asset);
        }

        private async Task<IAsset> loadAssetFromAssetDatabase(string asset_path, Type type) 
        {
            await UniTask.DelayFrame(1);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(asset_path, type);
            return new EditorAsset(asset);
        }


        #endregion
#endif

    }

    /// <summary>
    /// Get download url of Web Assets
    /// </summary>
    /// <param name="platform_name"></param>
    /// <param name="assetBundleName"></param>
    /// <param name="group"></param>
    /// <param name="isExtensionGroup"></param>
    /// <returns></returns>
    public delegate string GetWebAssetDownloadUrlDelegate(string platform_name, string assetBundleName, ref VFSGroup group, bool isExtensionGroup);
    public delegate string GetFileHashDownloadUrlDalegate(string platform_name, bool isExtensionGroup, string groupName);
    public delegate string GetAssetBundleManifestDownloadUrlDalegate(string platform_name, bool isExtensionGroup, string groupName);

}