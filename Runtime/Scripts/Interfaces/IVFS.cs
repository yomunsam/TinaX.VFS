using TinaX.VFSKitInternal;

namespace TinaX.VFSKit
{
    public interface IVFS
    {
        string ConfigPath { get; set; }
        AssetLoadType ConfigLoadType { get; }
        VFSCustomizable Customizable { get; }

        void RunTest();
    }
}

