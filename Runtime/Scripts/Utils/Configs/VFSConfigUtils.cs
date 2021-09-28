using System;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.ConfigTpls;
using UnityEngine;

namespace TinaX.VFS.Utils.Configs
{
    /// <summary>
    /// VFS 配置 相关的Util
    /// </summary>
    public static class VFSConfigUtils
    {
        /// <summary>
        /// 根据VFS配置资产创建配置模板对象（不作多余的处理）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CreateAndMapToVFSConfigTpl(ref VFSConfigAsset source, out VFSConfigTpl target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            //这里借用Json来处理，目的是深拷贝
            var json_str = JsonUtility.ToJson(source);
            target = JsonUtility.FromJson<VFSConfigTpl>(json_str);
        }
    }
}
