using System.Collections.Generic;

namespace TinaX.VFS.Assets
{
    /// <summary>
    /// 资产管理器
    /// </summary>
    public class VFSAssetManager
    {
        /// <summary>
        /// Key是LoadPath
        /// </summary>
        private readonly Dictionary<string, VFSAsset> m_AssetDict = new Dictionary<string, VFSAsset>();

        public bool TryGet(string loadPath, out VFSAsset asset)
        {
            lock (this)
            {
                return m_AssetDict.TryGetValue(loadPath, out asset);
            }
        }
    }
}
