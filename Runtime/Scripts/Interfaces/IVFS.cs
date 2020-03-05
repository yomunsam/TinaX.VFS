using TinaX.VFSKitInternal;

namespace TinaX.VFSKit
{
    public interface IVFS
    {
        string ConfigPath { get; set; }
        AssetLoadType ConfigLoadType { get; }
        VFSCustomizable Customizable { get; }
        string DownloadWebAssetUrl { get; }
        XRuntimePlatform Platform { get; }
        string PlatformText { get; }

        VFSGroup[] GetAllGroups();
        void RunTest();
        bool TryGetGroup(string groupName, out VFSGroup group);
    }
}

