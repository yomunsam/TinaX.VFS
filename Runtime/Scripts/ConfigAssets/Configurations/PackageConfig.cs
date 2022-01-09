using System;
using System.Collections.Generic;

namespace TinaX.VFS.ConfigAssets.Configurations
{
    /// <summary>
    /// VFS 资产包 配置
    /// </summary>
    [Serializable]
    public class PackageConfig
    {
        public List<GroupConfig> Groups = new List<GroupConfig>();


        public PackageConfig()
        {
#if UNITY_EDITOR
            if(Groups.Count == 0)
            {
                Groups.Add(new GroupConfig
                {
                    Name = "Default"
                });
            }
#endif
        }
    }
}
