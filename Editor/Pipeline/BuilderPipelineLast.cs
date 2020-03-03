using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;

namespace TinaXEditor.VFSKit.Pipeline.Builtin
{
    public class BuilderPipelineLast : IBuildHandler
    {
        public bool BeforeAssetBundleFileSavedByGroup(ref VFSGroup group, string assetBundleFileName, string assetName, ref FileStream fileStream)
        {
            return true;
        }

        /// <summary>
        /// 在设置AssetBundle之前
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <param name="assetQueryResult"></param>
        public bool BeforeSetAssetBundleSign(ref string assetbundleName, ref AssetsStatusQueryResult assetQueryResult)
        {
            return true;
        }
    }
}
