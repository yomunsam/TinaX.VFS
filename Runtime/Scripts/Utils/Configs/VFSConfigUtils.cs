using System;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.SerializableModels;
using UnityEngine;

#nullable enable
namespace TinaX.VFS.Utils
{
    /// <summary>
    /// VFS 配置 相关的Util
    /// </summary>
    public static class VFSConfigUtils
    {

        /// <summary>
        /// 将给定的VFS配置资产 Map到 VFSConfigModel
        /// </summary>
        /// <param name="configAsset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static VFSConfigModel MapToVFSConfigModel(VFSConfigAsset configAsset)
        {
            MapToVFSConfigModel(in configAsset, out var tpl);
            return tpl;
        }

        public static void MapToVFSConfigModel(in VFSConfigAsset configAsset, out VFSConfigModel tpl)
        {
            if (configAsset == null)
                throw new ArgumentNullException(nameof(configAsset));

            //借用Unity的Json库做深拷贝
            var jsonStr = JsonUtility.ToJson(configAsset);
            tpl = JsonUtility.FromJson<VFSConfigModel>(jsonStr);
        }
    }
}
