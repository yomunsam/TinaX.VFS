using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX;
using TinaX.Services;
using TinaX.VFSKit.Const;
using TinaX.VFSKit.Exceptions;
using TinaX.VFSKitInternal;
using System;
using UnityEngine;
using TinaX.VFSKitInternal.Utils;

namespace TinaX.VFSKit
{
    public class VFSKit : IVFS , IVFSInternal , IAssetService
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
        private List<string> mWhitelistFolderPaths = new List<string>();

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
            // load config by xconfig | VFS not enable, so vfs config can not load by vfs.
            mConfig = XConfig.GetConfig<VFSConfigModel>(ConfigPath);
            if(mConfig == null)
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
                    mGroups.Add(new VFSGroup(groupOption));
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

    }
}