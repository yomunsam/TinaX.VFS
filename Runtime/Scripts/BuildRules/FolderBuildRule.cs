using System;
using System.Collections.Generic;

namespace TinaX.VFS.BuildRules
{
    [Serializable]
    public struct FolderBuildRule
    {
        public string Path;
        public FolderBuildType FolderBuildType;
        public List<string> Tags;
    }
}
