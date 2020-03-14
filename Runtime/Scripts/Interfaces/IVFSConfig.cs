using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKit;

namespace TinaX.VFSKitInternal
{
    public interface IVFSConfig
    {
        VFSGroupOption[] PGroups { get; set; }
        bool PEnableVFS { get; set; }
        string PAssetBundleFileExtension { get; set; }
        string[] GlobalVFS_Ignore_Path_Item_Lower { get; }
        string[] PGlobalVFS_Ignore_ExtName { get; set; }
        string[] PGlobalVFS_Ignore_Path_Item { get; set; }
        bool PInitWebVFSOnStart { get; set; }
        string PDefaultWebVFSBaseUrl { get; set; }

        string[] GetGlobalVFS_Ignore_Path_Item(bool lower = false, bool forceRefresh = false);
    }
}
