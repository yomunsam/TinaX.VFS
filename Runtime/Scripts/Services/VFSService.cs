using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TinaX.Exceptions;
using TinaX.Options;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Internal;
using TinaX.VFS.Options;
using UnityEngine;
using UObject = UnityEngine.Object;

#nullable enable
namespace TinaX.VFS.Services
{
    public class VFSService : IVFS, IVFSInternal
    {
        //------------固定字段--------------------------------------------------------------------------
        private readonly VFSOptions m_Option;

        //------------构造方法--------------------------------------------------------------------------
        public VFSService(IOptions<VFSOptions> option)
        {
            this.m_Option = option.Value;
        }

        //------------私有字段--------------------------------------------------------------------------
        private bool m_Initialized;

        /// <summary>
        /// 在编辑器下，使用UnityEditor.AssetDatabase 加载资产？
        /// </summary>
        private bool m_LoadAssetByEditor = false;

        //------------公开方法--------------------------------------------------------------------------



        //------------公开方法--------------------------------------------------------------------------

        public void Release(UObject asset)
        {
            throw new NotImplementedException();
        }


        #region 生命周期

        public async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (m_Initialized)
                return;
            if (!m_Option.Enable)
                return;

            var vfs_config_asset = await LoadVFSConfigAssetAsync(m_Option.ConfigAssetLoadPath, cancellationToken);
            if(vfs_config_asset == null)
            {
                throw new XException($"Failed to load configuration assets \"{m_Option.ConfigAssetLoadPath}\" ");
            }
            //if (!vfs_config_asset.Enable)
            //{
            //    Debug.LogFormat("VFS is not enabled according to the configuration");
            //    return;
            //}

            //资产加载方式
#if UNITY_EDITOR
            m_LoadAssetByEditor = true; //编辑器下，默认资产加载方式是UnityEditor.AssetDatabase
#endif

            await UniTask.CompletedTask;


            m_Initialized = true;
        }



        #endregion



        /// <summary>
        /// 运行时加载VFS的配置文件
        /// </summary>
        /// <param name="loadPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private UniTask<VFSConfigAsset> LoadVFSConfigAssetAsync(string loadPath, CancellationToken cancellationToken)
        {
            /*
             * TinaX.Core中的IConfigAssetService依赖IAssetService来加载AssetBundle中的配置文件
             * 而VFS正是IAssetService的实现者，因此在VFS的启动过程中，不能用IConfigAssetService来加载资产，
             * VFS的配置资产需要自行解决加载问题
             */

            return UniTask.FromResult<VFSConfigAsset>(null);

        }

    }
}
