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
                if (mManifest_VirtualDisk != null) return mManifest_VirtualDisk;
                return mManifest_StreamingAssets;
            }
        }

        private FilesHashBook mFileHash_StreamingAssets;
        private FilesHashBook mFileHash_VirtualDisk;
        private FilesHashBook mFileHash_WebVFS;

        private string webVfs_asset_download_base_url;
        private bool webvfs_download_base_url_modify = false; //如果被profile或者手动修改过的话，这里为true

        /// <summary>
        /// 获取WebAssets的Url请调用这里的委托变量
        /// </summary>
        internal GetWebAssetUrlDelegate GetWebAssetUrl;
        /// <summary>
        /// 获取hash file 的url请调用这个
        /// </summary>
        internal GetFileHashUrlDalegate GetWebFileHashBookUrl;
        internal BundlesManager Bundles { get; private set; } = new BundlesManager();
        internal AssetsManager Assets { get; private set; } = new AssetsManager();
        internal ExtensionGroupsManager ExtensionGroups { get; private set; }

        private bool mInited = false;
        private bool mWebVFSReady = false;


        public VFSKit()
        {
            ExtensionGroups = new ExtensionGroupsManager(this); 
            Customizable = new VFSCustomizable(this);
            this.GetWebAssetUrl = getWebAssetUrl;
            this.GetWebFileHashBookUrl = getWebFilesHashUrl;


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
            mVirtualDisk_MainPackageFolderPath = Path.Combine(VirtualDiskPath, VFSConst.VFS_FOLDER_MAIN);
            mVirtualDisk_DataFolderPath = Path.Combine(VirtualDiskPath, VFSConst.VFS_FOLDER_DATA);
            mVirtualDisk_ExtensionGroupRootFolderPath = Path.Combine(VirtualDiskPath, VFSConst.VFS_FOLDER_EXTENSION);

            #region init virtual disk assetbundleManifest
            //this path can be load by "System.IO" in any platform
            string virtualdisk_manifest_path = VFSUtil.GetAssetBundleManifestInPackage(mVirtualDisk_MainPackageFolderPath);
            if (File.Exists(virtualdisk_manifest_path))
            {
                try
                {
                    string json_text = File.ReadAllText(virtualdisk_manifest_path, Encoding.UTF8);
                    var manifest_obj = JsonUtility.FromJson<BundleManifest>(json_text);
                    mManifest_VirtualDisk = new XAssetBundleManifest(manifest_obj);
                    manifest_obj = null;
                }
                catch { }
            }
            #endregion

            #region init virtual disk assetbundle hash book
            string virtualdisk_hash_path = VFSUtil.GetAssetBundleFileHashBookInPackage(mVirtualDisk_MainPackageFolderPath);
            if (File.Exists(virtualdisk_hash_path))
            {
                try
                {
                    string json = File.ReadAllText(virtualdisk_manifest_path, Encoding.UTF8);
                    mFileHash_VirtualDisk = JsonUtility.FromJson<FilesHashBook>(json);
                }catch { }
            }
            #endregion

            //init vfs packages in streamingassets
            //main package 's assetbundleManifest
            bool init_streamingassets = true;
#if UNITY_EDITOR
            if (mLoadByAssetDatabaseInEditor) init_streamingassets = false;
#endif
            if (init_streamingassets)
            {
                Debug.Log("vfs start 开始处理streamingassets");
                #region StramingAssets AssetBundleManifest
                string streaming_manifest_path = VFSUtil.GetAssetBundleManifestInPackage(mStreamingAssets_MainPackageFolderPath);
                try
                {
                    string streaming_manifest_json = await LoadTextFromStreamingAssetsAsync(streaming_manifest_path);
                    if (!streaming_manifest_json.IsNullOrEmpty())
                    {
                        Debug.Log(streaming_manifest_json);
                        var streaming_manifest_obj = JsonUtility.FromJson<BundleManifest>(streaming_manifest_json);
                        mManifest_StreamingAssets = new XAssetBundleManifest(streaming_manifest_obj);
                        streaming_manifest_obj = null;
                    }
                }
                catch (FileNotFoundException) { /* do nothing */ }
                catch (VFSException e)
                {
                    mStartException = e;
                    return false;
                }

                #endregion

                #region StreamingAssets assetbundle files hash .
                string streaming_hash_path = VFSUtil.GetAssetBundleFileHashBookInPackage(mStreamingAssets_MainPackageFolderPath);
                try
                {
                    var json = await LoadTextFromStreamingAssetsAsync(streaming_hash_path);
                    mFileHash_StreamingAssets = JsonUtility.FromJson<FilesHashBook>(json);

                }
                catch (FileNotFoundException) { /* do nothing */ }
                catch (VFSException e)
                {
                    mStartException = e;
                    return false;
                }
                #endregion

            }

            Debug.Log("vfs start 开始处理webvfs ");
            bool need_init_webvfs = false;
            if (mConfig.EnableWebVFS)
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
                    return false;
                }
            }

            Debug.Log(" vfs start 结束");
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
            var hash_url = new Uri(this.GetWebHashsFileDownloadUrl(PlatformText, false));
            try
            {
                var json = await LoadTextFromWebAsync(hash_url,5);
                mFileHash_WebVFS = JsonUtility.FromJson<FilesHashBook>(json);
            }
            catch(FileNotFoundException e)
            {
                throw new VFSException("Init WebVFS Failed: Cannot download Hashs file from: " + e.Path);
            }

            //Extensions //改动，Extension需要主动启用
            //foreach(var group in mGroups)
            //{
            //    if(group.ExtensionGroup && (group.HandleMode == GroupHandleMode.LocalOrRemote || group.HandleMode == GroupHandleMode.RemoteOnly))
            //    {
            //        try
            //        {
            //            var url = new Uri(this.GetWebHashsFileDownloadUrl(PlatformText, true, group.GroupName));
            //            var json = await LoadTextFromWebAsync(url, 5);
            //            var obj = JsonUtility.FromJson<FilesHashBook>(json);
            //            lock (mDict_FileHash_WebVFS_ExtensionGroups)
            //            {
            //                if (mDict_FileHash_WebVFS_ExtensionGroups.ContainsKey(group.GroupName))
            //                    mDict_FileHash_WebVFS_ExtensionGroups[group.GroupName] = obj;
            //                else
            //                    mDict_FileHash_WebVFS_ExtensionGroups.Add(group.GroupName, obj);
            //            }

            //        }
            //        catch(FileNotFoundException e)
            //        {
            //            // do nothing
            //            //throw new VFSException("Init WebVFS Failed: Cannot download Hashs file from: " + e.Path);
            //        }
            //    }
            //}
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
            //Debug.Log("关于在StreamingAssets中加载文件的测试");
            //string file_path = Path.Combine(Application.streamingAssetsPath, "test.txt");
            //byte[] bytes = await this.LoadFileFromStreamingAssetsAsync(file_path);
            //string text = System.Text.Encoding.UTF8.GetString(bytes);
            //Debug.Log("text:" + text);

            //Debug.Log("新增的测试：加载一个大的文件");
            //await this.LoadFileFromStreamingAssetsAsync(Path.Combine(Application.streamingAssetsPath, "m.flac"));
            //Debug.Log("结束");

            Debug.Log("关于直接加载AssetBundle的测试");
            await loadAssetAsync<Sprite>("Assets/MyApp/Local/imgs/myImg1.jpg");
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
            Debug.Log("喵载入文本:" + path);
            var req = UnityWebRequest.Get(path);
            await req.SendWebRequest();
            if (req.isHttpError)
            {
                if (req.responseCode == 404)
                    throw new Exceptions.FileNotFoundException($"Failed to load file from StreamingAssets, file path:{path}", path);
            }
            return VFSUtil.RemoveInvalidHead(req.downloadHandler.text);
        }

        private async UniTask<string> LoadTextFromWebAsync(Uri uri, int timeout = 3, Encoding encoding = null)
        {
            Debug.Log("喵下载文本:" + uri + "  timeout:" + timeout);
            var req = UnityWebRequest.Get(uri);
            req.timeout = timeout;
            var op =  req.SendWebRequest();
            Debug.Log("下载文本：" + op.GetHashCode());
            Task waitTask = Task.Run(async () => { await op; });
            var t = await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(timeout)));
            if(t != waitTask)
            {
                Debug.LogError("超时");
            }

            if (req.isNetworkError || req.isHttpError)
            {
                if (req.responseCode == 404)
                    throw new FileNotFoundException("Failed to get text from web : " + uri.ToString(), uri.ToString());
                else
                    throw new VFSException("Failed to get text from web:" + uri.ToString());
            }
            if (encoding == null)
                return req.downloadHandler.text;
            else
                return encoding.GetString(req.downloadHandler.data);
        }

        #region VFS 加载流程

        private async UniTask<T> loadAssetAsync<T>(string assetPath) where T: UnityEngine.Object
        {
            if(QueryAsset(assetPath, out var result , out var group))
            {
                //可被加载
                //那就加载吧，首先加载Bundle
                VFSBundle bundle = await loadAssetBundleAndDependenciesAsync(result.AssetBundleName, group, true);
                //然后加载Asset
                Debug.Log("加载完成");
                foreach(var a in bundle.AssetBundle.GetAllAssetNames())
                {
                    Debug.Log(a);
                }
                return default;
            }
            else
            {
                throw new VFSException((IsChinese ? "被加载的asset的路径是无效的，它不在VFS的管理范围内" : "The asset path you want to load is valid. It is not under the management of VFS") + "Path:" + assetPath, VFSErrorCode.ValidLoadPath);
            }
        }

        //private UniTask<VFSAsset> loadAssetFromAssetBundle<T>(string assetPath)
        //{
        //}

        #endregion 

        #region 加载AssetBundle_Async

        /// <summary>
        /// 加载AssetBundle和它的依赖， 异步总入口
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        private async UniTask<VFSBundle> loadAssetBundleAndDependenciesAsync(string assetbundleName, VFSGroup group, bool counter = true)
        {

            //先加载依赖吧
            string[] DependenciesNames = null;
            if (group.ExtensionGroup)
            {
                var egroup = (VFSExtensionGroup)group;
                if (egroup.AssetBundleManifest != null)
                {
                    DependenciesNames = egroup.AssetBundleManifest.GetAllDependencies(assetbundleName);
                }
                else
                {
                    Debug.LogError((IsChinese ? "[TinaX.VFS] VFS尝试加载AssetBundle，但是当前没有任何有效的AssetBundleManifest文件。" : "[TinaX.VFS]VFS tried to load AssetBundle, but there are currently no valid AssetBundleManifest files.") + "Path:" + assetbundleName + "\nGroup:" + egroup.GroupName);
                }
            }
            else
            {
                if (mAssetBundleManifest != null)
                {
                    DependenciesNames = mAssetBundleManifest.GetAllDependencies(assetbundleName);
                }
                else
                {
                    Debug.LogError((IsChinese ? "[TinaX.VFS] VFS尝试加载AssetBundle，但是当前没有任何有效的AssetBundleManifest文件。" : "[TinaX.VFS]VFS tried to load AssetBundle, but there are currently no valid AssetBundleManifest files.") + "Path:" + assetbundleName);
                }
            }

            List<VFSBundle> dependencies_bundle = new List<VFSBundle>();
            List<UniTask<VFSBundle>> dependency_task = new List<UniTask<VFSBundle>>();
            if(DependenciesNames != null && DependenciesNames.Length > 0)
            {
                foreach(var name in DependenciesNames)
                {
                    if(this.TryGetGroupByAssetBundleName(name,out var _group))
                    {
                        var task = loadAssetBundleAndDependenciesAsync(name, _group, counter);
                        dependency_task.Add(task);
                    }
                    else
                    {
                        Debug.LogError("[TinaX.VFS]Cannot found assetbundle's depencency in any group, assetbundle:" + assetbundleName +" depencency:" + name );
                    }
                }

                await UniTask.WhenAll(dependency_task);
                foreach(var task in dependency_task)
                {
                    dependencies_bundle.Add(task.Result);
                }
            }

            return await loadAssetBundleAsync(assetbundleName, dependencies_bundle, group, counter);
        }

        /// <summary>
        /// 加载一个AssetBundle，不考虑依赖， 异步总入口
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        private async UniTask<VFSBundle> loadAssetBundleAsync(string assetBundleName, List<VFSBundle> dependencise, VFSGroup group, bool counter = true)
        {
            //检查bundle是不是已经加载了
            if(IsBundleLoadedOrLoading(assetBundleName,out var _bundle))
            {
                if (counter) _bundle.Retain();
                lock (_bundle)
                {
                    if (_bundle.LoadState == AssetLoadState.Loaded)
                        return _bundle;
                }
                await _bundle.LoadTask;
                return _bundle;
            }

            //那么现在得考虑下加载它了
            var my_bundle = new VFSBundle();
            my_bundle.AssetBundleName = assetBundleName;
            if (dependencise != null)
                my_bundle.Dependencies = dependencise;

            my_bundle.LoadedPath = GetAssetBundleLoadPath(assetBundleName,ref group,out var vdisk_path);
            my_bundle.ABLoader = group.ABLoader;
            my_bundle.DownloadTimeout = this.DownloadWebAssetTimeout;
            my_bundle.VirtualDiskPath = vdisk_path;
            my_bundle.GroupHandleMode = group.HandleMode;

            this.Bundles.Register(my_bundle);

            //加载
            await my_bundle.Load();
            return my_bundle;
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

        public bool TryGetGroupByAssetBundleName(string assetbundle,out VFSGroup group)
        {
            foreach(var g in mGroups)
            {
                if (g.IsAssetBundleMatch(assetbundle))
                {
                    group = g;
                    return true;
                }
            }

            foreach(var g in this.ExtensionGroups.mGroups)
            {
                if (g.IsAssetBundleMatch(assetbundle))
                {
                    group = g;
                    return true;
                }
            }
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

        private bool IsAssetLoaded(string lower_path)
        {
            return false;   //TODO: 检查资源是否已加载
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
                    if (group.ExtensionGroup)
                    {
                        var eGroup = (VFSExtensionGroup)group;
                        if (eGroup.FileHash_StreamingAssets != null)
                            eGroup.FileHash_StreamingAssets.TryGetFileHashValue(assetbundle, out hash_streaming);

                        //尝试找到它在remote的hash
                        if (eGroup.FileHash_Remote != null)
                        {
                            if (eGroup.FileHash_Remote.TryGetFileHashValue(assetbundle, out var remote_hash))
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
                    else
                    {
                        if (mFileHash_StreamingAssets != null)
                            mFileHash_StreamingAssets.TryGetFileHashValue(assetbundle, out hash_streaming);

                        //找找remote的hash
                        if(mFileHash_WebVFS != null)
                        {
                            if(mFileHash_WebVFS.TryGetFileHashValue(assetbundle,out string remote_hash))
                            {
                                if (!hash_vdisk.Equals(remote_hash))
                                {
                                    if(!hash_streaming.IsNullOrEmpty())
                                    {
                                        if(hash_streaming.Equals(remote_hash))
                                            return VFSUtil.GetAssetPath(false, mStreamingAssets_ExtensionGroupRootFolderPath, assetbundle);
                                        else
                                            return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                                    }
                                    else
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
                
                if (group.ExtensionGroup)
                {
                    var eGroup = (VFSExtensionGroup)group;
                    if(eGroup.FileHash_StreamingAssets == null)
                    {
                        return asset_path_streamingassets; //因为无法获取到streamingassets文件中的hash，所以直接不比较了
                    }
                    
                    if(eGroup.FileHash_StreamingAssets.TryGetFileHashValue(assetbundle,out var stream_hash))
                    {
                        if (eGroup.FileHash_Remote != null)
                        {
                            if (eGroup.FileHash_Remote.TryGetFileHashValue(assetbundle, out var remote_hash))
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
                else
                {
                    if (mFileHash_StreamingAssets == null)
                        return asset_path_streamingassets;

                    if(mFileHash_StreamingAssets.TryGetFileHashValue(assetbundle,out var stream_hash))
                    {
                        if(mFileHash_WebVFS.TryGetFileHashValue(assetbundle,out var remote_hash))
                        {
                            if(!remote_hash.Equals(stream_hash))
                                return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                        }
                    }
                    else
                        return this.GetWebAssetDownloadUrl(PlatformText, assetbundle, ref group);
                }
            }
            
            return asset_path_streamingassets;
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

        private string GetWebHashsFileDownloadUrl(string platform, bool isExtensionGroup, string groupName = null)
        {
            return this.DownloadWebAssetUrl + this.GetWebFileHashBookUrl(platform, isExtensionGroup, groupName);
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
        private string getWebAssetUrl(string platform_name, string assetBundleName, ref VFSGroup group, bool isExtensionGroup)
        {
            if (isExtensionGroup)
                return $"{platform_name}/{group.GroupName}/{assetBundleName}";
            else
                return $"{platform_name}/main/{assetBundleName}";
        }

        private string getWebFilesHashUrl(string platform_name, bool isExtensionGroup, string groupName = null)
        {
            if (isExtensionGroup)
                return $"{platform_name}/{groupName}/{VFSConst.ABsHashFileName}";
            else
                return $"{platform_name}/main/{VFSConst.ABsHashFileName}";
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
    public delegate string GetFileHashUrlDalegate(string platform_name, bool isExtensionGroup, string groupName = null);

}