using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;

namespace TinaXEditor.VFSKit
{
    public interface IVFSBuilder
    {
        TinaX.VFSKit.VFSConfigModel Config { get; }
        bool EnableTipsGUI { get; set; }
        bool CopyToStreamingAssetsFolder { get; set; }
        bool ClearAssetBundleSignBeforeBuild { get; set; }
        bool ClearAssetBundleSignAfterBuild { get; set; }
        bool ForceRebuild { get; set; }
        bool ClearOutputFolder { get; set; }

        void Build(TinaX.XRuntimePlatform platform ,AssetCompressType compressType);
        void RefreshAssetBundleSign();
        IVFSBuilder SetConfig(VFSConfigModel config);
        IVFSBuilder UseProfile(string profileName);
    }
}
