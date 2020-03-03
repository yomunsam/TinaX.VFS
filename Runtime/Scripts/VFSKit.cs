using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX;
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

namespace TinaX.VFSKit
{
    public class VFSKit : IVFS, IVFSInternal, IAssetService
    {
        public string ConfigPath { get; set; } = VFSConst.ConfigFilePath_Resources;
        public AssetLoadType ConfigLoadType { get; private set; } = AssetLoadType.Resources;
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

        public VFSKit()
        {

        }

        /// <summary>
        /// 启动，如果初始化失败，则返回false.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
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

            //load

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

            await Task.Delay(0);
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

        public void RunTest()
        {
            Debug.Log("关于在StreamingAssets中加载文件的测试");
            string file_path = Path.Combine(Application.streamingAssetsPath, "test.txt");
            byte[] bytes = this.LoadFileFromStreamingAssetsAsync(file_path).Result;
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("text:" + text);
        }


        private async Task<byte[]> LoadFileFromStreamingAssetsAsync(string path)
        {
            var req = UnityWebRequest.Get(path);
            var operation = req.SendWebRequest();
            await Task.Delay(0);
            var result = await operation;

            if (result.isHttpError)
            {
                if (result.responseCode == 404)
                    throw new Exceptions.FileNotFoundException($"Failed to load file from StreamingAssets, file path:{path}", path);
            }
            return result.downloadHandler.data;
        }






        //private IEnumerator LoadFileFromStreamingAssetsAsync(string path, Action<byte[]> callback)
        //{
        //    var req = UnityWebRequest.Get(path);
        //    yield return req.SendWebRequest();
        //    if (req.isHttpError)
        //    {
        //        if (req.responseCode == 404)
        //            throw new Exceptions.FileNotFoundException($"Failed to load file from StreamingAssets, file path:{path}", path);
        //    }
        //}


    }
}