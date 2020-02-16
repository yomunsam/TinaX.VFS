using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    [Serializable]
    public struct FolderBuildRule
    {
        public string FolderPath;
        public FolderBuildType BuildType;
        public FolderBuildDevelopType DevType;
    }
}
