using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKitInternal;

namespace TinaX.VFSKit
{
    public class VFSExtensionGroup : VFSGroup
    {
        public FilesHashBook FileHash_StreamingAssets { get; private set; }
        public FilesHashBook FileHash_VirtualDisk { get; private set; }
        public FilesHashBook FileHash_Remote { get; private set; }

        public XAssetBundleManifest Manifest_StreamingAssets { get; private set; }
        public XAssetBundleManifest Manifest_VirtualDisk { get; private set; }

        public XAssetBundleManifest AssetBundleManifest {
            get
            {
                if (Manifest_VirtualDisk != null) return Manifest_VirtualDisk;
                return Manifest_StreamingAssets;
            }
        }
        //public XAssetBundleManifest Manifest_Remote { get; private set; } //Remote没有Manifest, Remote资源和VirtualDisk是一起的

    }
}
