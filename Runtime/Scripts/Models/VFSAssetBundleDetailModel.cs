using System;

namespace TinaX.VFS.Models
{
    [System.Serializable]
    public class VFSAssetBundleDetailModel
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
            get => h;
            set => h = value;
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
        private string h;

        [UnityEngine.SerializeField]
        private uint c;

        [UnityEngine.SerializeField]
        private string[] d;
    }
}
