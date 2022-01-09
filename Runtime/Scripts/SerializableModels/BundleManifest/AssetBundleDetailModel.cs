namespace TinaX.VFS.SerializableModels.BundleManifest
{
    /// <summary>
    /// 可序列化模型 - AssetBundle 细节
    /// </summary>
    [System.Serializable]
    public class AssetBundleDetailModel
    {
        /// <summary>
        /// AssetBundle Name
        /// </summary>
        public string AssetBundleName
        {
            get => n;
            set => n = value;
        }

        public string MD5
        {
            get => m;
            set => m = value;
        }

        public uint Crc
        {
            get => c;
            set => c = value;
        }

        public string[] Dependencies
        {
            get => d;
            set => d = value;
        }


        [UnityEngine.SerializeField]
        private string n;

        [UnityEngine.SerializeField]
        private string m;

        [UnityEngine.SerializeField]
        private uint c;

        [UnityEngine.SerializeField]
        private string[] d;
    }
}
