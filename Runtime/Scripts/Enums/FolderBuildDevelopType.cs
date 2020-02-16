using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    [Serializable]
    public enum FolderBuildDevelopType
    {
        normal,
        editor_only,
        /// <summary>
        /// when enable "develop mode" in "VFS Profile", 
        /// </summary>
        develop_mode_only,
    }
}
