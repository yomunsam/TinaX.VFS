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
        public bool BeforeAssetBundleFileSavedByGroup(ref VFSEditorGroup group, string assetBundleFileName, ref FileStream fileStream) => true;

        public bool BeforeSetAssetBundleSign(ref string assetbundleName, ref AssetsStatusQueryResult assetQueryResult) => true;
    }
}
