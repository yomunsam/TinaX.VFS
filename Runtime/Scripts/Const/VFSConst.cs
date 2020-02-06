using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinaX.VFSKit.Const
{
    public static class VFSConst
    {
        public const string ServiceName = "TinaX.VFS";

        public const string ConfigFileName = "VFSConfig";

        public static string ConfigFilePath_Resources = $"{TinaX.Const.FrameworkConst.Framework_Configs_Folder_Path}/{ConfigFileName}";

        public static System.Type[] IgnoreType =
        {
#if UNITY_EDITOR
            typeof(UnityEditor.MonoScript),
            typeof(UnityEditor.DefaultAsset),
#endif
        };


    }
}


