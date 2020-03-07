using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// 可自定义的
    /// </summary>
    public class VFSCustomizable
    {
        private readonly TinaX.VFSKit.VFSKit mVFS;
        public VFSCustomizable(TinaX.VFSKit.VFSKit vfs) { mVFS = vfs; }

        /// <summary>
        /// Get download url of web asset.
        /// </summary>
        /// <param name="func"></param>
        public void GetWebAssetUrl(TinaX.VFSKit.GetWebAssetUrlDelegate func)
        {
            mVFS.GetWebAssetUrl = func;
        }

        public void GetWebFileHashUrl(TinaX.VFSKit.GetFileHashUrlDalegate func)
        {
            mVFS.GetWebFileHashBookUrl = func;
        }
    }
}
