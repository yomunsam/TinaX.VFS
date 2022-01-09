using System;

namespace TinaXEditor.VFS.SerializableModels
{
    /// <summary>
    /// 可序列化模型 - 资产Hash
    /// </summary>
    [Serializable]
    public class AssetsHashModel
    {
        public AssetsHashItemModel[] Items;
    }

    [Serializable]
    public class AssetsHashItemModel
    {
        public string AssetPath
        {
            get => a;
            set => a = value;
        }

        public string Hash
        {
            get => h;
            set => h = value;
        }

        /// <summary>
        /// AssetBundle
        /// </summary>
        public string Bundle
        {
            get => ab;
            set => ab = value;
        }

        /// <summary>
        /// AssetBundle 变体
        /// </summary>
        public string BundleVariant
        {
            get => abv;
            set => abv = value;
        }

        public string FileNameInAssetBundle
        {
            get => fia;
            set => fia = value;
        }

        [UnityEngine.SerializeField]
        private string a;

        [UnityEngine.SerializeField]
        private string h;

        [UnityEngine.SerializeField]
        private string ab;

        [UnityEngine.SerializeField]
        private string abv;
        
        [UnityEngine.SerializeField]
        private string fia;
    }
}
