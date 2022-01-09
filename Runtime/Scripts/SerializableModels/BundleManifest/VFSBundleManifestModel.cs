using System;

namespace TinaX.VFS.SerializableModels.BundleManifest
{
    /// <summary>
    /// 可序列化模型 - VFS AssetBundle Manifest
    /// </summary>
    [Serializable]
    public class VFSBundleManifestModel
    {
        public int Version;
        public AssetBundleDetailModel[] Bundles;
    }
}
