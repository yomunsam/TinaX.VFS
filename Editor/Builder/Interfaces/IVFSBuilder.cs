using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;
using TinaXEditor.VFSKit.Pipeline;

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

        /// <summary>
        /// Builder will automatically reflect all available handler classes | Builder 将会自动反射出所有可用的Handler类.
        /// </summary>
        /// <returns></returns>
        IVFSBuilder UseAutoPipeline();
        IVFSBuilder UsePipeline(BuilderPipeline pipeline);
        IVFSBuilder UseProfile(string profileName);
    }
}
