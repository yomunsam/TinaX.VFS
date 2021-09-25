using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TinaX.Systems.Pipeline;
using TinaX.VFS.ConfigTpls;
using TinaX.VFS.Extensions;
using TinaX.VFS.Pipelines.LoadVFSConfigAsset;
using UnityEngine;

namespace TinaX.VFS.ConfigAssets.Loader
{
    /// <summary>
    /// VFS配置资产 加载器
    /// </summary>
    public class VFSConfigAssetLoader
    {
        private readonly XPipeline<ILoadVFSConfigAssetHandler> m_LoadVFSConfigAssetAsyncPipeline = new XPipeline<ILoadVFSConfigAssetHandler>();

        public VFSConfigAssetLoader()
        {
            LoadVFSConfigAssetAsyncPipelineConfigure(ref m_LoadVFSConfigAssetAsyncPipeline);
        }


        private void LoadVFSConfigAssetAsyncPipelineConfigure(ref XPipeline<ILoadVFSConfigAssetHandler> pipeline)
        {
            //准备从PersistentDataPath加载
            pipeline.Use(LoadVFSConfigAssetHandlerNameConsts.ReadyLoadFromPersistentDataPath, (LoadVFSConfigAssetContext context, ILoadVFSConfigAssetHandler next, CancellationToken cancellationToken) =>
            {
                return UniTask.CompletedTask;
            });
        }

        /// <summary>
        /// 把VFS配置资产的数据给弄到配置模板里（不作多余的处理）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void MapToVFSConfigTpl(ref VFSConfigAsset source, ref VFSConfigTpl target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            //这里借用Json来处理，目的是深拷贝
            var json_str = JsonUtility.ToJson(source);
            JsonUtility.FromJsonOverwrite(json_str, target);
        }

        /// <summary>
        /// 根据VFS配置资产创建配置模板对象（不作多余的处理）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void CreateAndMapToVFSConfigTpl(ref VFSConfigAsset source, out VFSConfigTpl target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            //这里借用Json来处理，目的是深拷贝
            var json_str = JsonUtility.ToJson(source);
            target = JsonUtility.FromJson<VFSConfigTpl>(json_str);
        }

        /// <summary>
        /// 预处理 VFS配置模板
        /// （只处理全局配置，包和组里的配置往后面处理）
        /// </summary>
        /// <param name="target"></param>
        public void PreProcessVFSConfigTpl(ref VFSConfigTpl target)
        {
            target.DefaultAssetBundleVariant = target.DefaultAssetBundleVariant.ToLower(); //涉及到路径的东西最后的lower

            //标准化 全局忽略文件扩展名
            StandardizedIgnoreExtensions(ref target.GlobalIgnoreExtensions);

            //标准化 全局忽略文件夹名
            StandardizedIgnoreFolderName(ref target.GlobalIgnoreFolderName);
        }


        #region 数据的预处理与标准化

        /// <summary>
        /// 数据预处理：标准化全局忽略扩展名
        /// </summary>
        private void StandardizedIgnoreExtensions(ref List<string> extensions)
        {
            for(var i = 0; i < extensions.Count; i++)
            {
                //小写和删首尾空格
                extensions[i] = extensions[i].ToLower().Trim();

                //确保前头有点号，比如配置项是"txt",标准化之后是".txt"
                if (!extensions[i].StartsWith("."))
                    extensions[i] = "." + extensions[i];
            }

            //去重复
            extensions = extensions.Distinct().ToList();
        }

        private void StandardizedIgnoreFolderName(ref List<string> folders)
        {
            if (folders.Count < 1)
                return;
            for(var i = folders.Count -1; i>=0; i--)
            {
                //小写和删首尾空格
                folders[i] = folders[i].ToLower().Trim();

                //确保前后有"/"符号，
                if (!folders[i].StartsWith("/"))
                    folders[i] = $"/{folders[i]}";
                if (!folders[i].EndsWith("/"))
                    folders[i] = folders[i] + "/";

                //如果有Assets 或 assets ，删掉
                if (folders[i] == "/assets/")
                    folders.RemoveAt(i); //因为这儿有Remove，所以这个判断必须放在循环的最后面
            }

            //去重复
            folders = folders.Distinct().ToList();
        }
        #endregion

    }
}
