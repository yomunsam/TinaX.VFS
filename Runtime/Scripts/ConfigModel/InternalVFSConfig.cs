using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// 存放“TinaX  VFSConfigModel”类中一些必须存在的内部配置  
    /// </summary>
    public static class InternalVFSConfig
    {

        public const string default_AssetBundle_ExtName = ".xab";


        public static string[] GlobalIgnoreExtName =
        {
            ".cs",
        };

        public static string[] GlobalIgnorePathItem =
        {
            "Editor",
        };

        public static string[] GlobalIgnorePathItemLower
        {
            get
            {
                string[] arr = new string[InternalVFSConfig.GlobalIgnorePathItem.Length];
                for(var i = 0; i < InternalVFSConfig.GlobalIgnorePathItem.Length; i++)
                {
                    arr[i] = InternalVFSConfig.GlobalIgnorePathItem[i].ToLower();
                }
                return arr;
            }
        }


    }
}
