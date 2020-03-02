using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit.Pipeline
{
    public interface IBuildHandler
    {
        void BeforeAssetBundleSign(ref string assetbundleName);
    }
}
