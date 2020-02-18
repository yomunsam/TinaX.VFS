using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit
{
    public interface IVFSBuilder
    {
        TinaX.VFSKit.VFSConfigModel Config { get; }
        bool EnableTipsGUI { get; set; }

        void RefreshAssetBundleSign(bool recordAssetHash = true);
    }
}
