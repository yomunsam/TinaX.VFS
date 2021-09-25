using System;
using System.Collections.Generic;

namespace TinaX.VFS.ConfigTpls
{
    /// <summary>
    /// 包（Package） 配置模板
    /// </summary>
    [Serializable]
    public class PackageConfigTpl
    {
        public List<GroupConfigTpl> Groups = new List<GroupConfigTpl>();


        public PackageConfigTpl()
        {
#if UNITY_EDITOR
            Groups.Add(new GroupConfigTpl
            {

            });
#endif
        }

    }
}
