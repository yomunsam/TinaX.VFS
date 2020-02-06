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
                throw new VFSException("Load VFS config failed, \nload type:" + ConfigLoadType.ToString() +"\nload path:" + ConfigPath,VFSErrorCode.LoadConfigFailed);
            }

            // init configs data.
            mGroups.Clear();
            if(mConfig.Groups != null)
            {
                foreach(var groupOption in mConfig.Groups)
                {
                    mGroups.Add(new VFSGroup(groupOption));
                }
            }

            //check groups config;
            for(var i = 0; i < mGroups.Count; i ++)
            {
                var group = mGroups[i];
                //检查文件夹路径冲突
                if(i == 0)
                {
                    //检查自身
                    if(group.CheckFolderConflict(out var paths))
                    {
                        throw new VFSException($"VFS Config Error : Groups's folderPaths configure conflict.\nGroup \"{group.GroupName}\" - FolderPath:{paths.FirstOrDefault()}", VFSErrorCode.ConfigureGroupsConflict);
                    }
                }
                else
                {
                    //检查自身
                    if (group.CheckFolderConflict(out var paths))
                    {
                        throw new VFSException($"VFS Config Error : Groups's folderPaths configure conflict.\nGroup \"{group.GroupName}\" - FolderPath:{paths.FirstOrDefault()}", VFSErrorCode.ConfigureGroupsConflict);
                    }

                    //检查与之前的组的路径冲突
                    foreach(var path in group.FolderPaths)
                    {
                        foreach(var _g in mGroups)
                        {
                            if (_g.CheckFolderConflict(path))
                            {
                                throw new VFSException($"VFS Config Error: Groups's folderPath configure conflict.\nGroup: \"{group.GroupName}\" - FolderPath:{path}  | Group:\"{_g.GroupName}\"");
                            }
                        }
                    }
                }
                //把当前group的folder 加入到全局的list 
                foreach(var path in group.FolderPaths)
                {
                    mWhitelistFolderPaths.Add(path.EndsWith("/") ? path : path + "/");
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
    }
}