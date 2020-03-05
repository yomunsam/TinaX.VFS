using UnityEngine;

namespace TinaX.VFSKit
{
    [System.Serializable]
    public class WebVFSNetworkConfig : ScriptableObject
    {
        public NetworkConfig[] Configs;

        [System.Serializable]
        public struct NetworkConfig
        {
            public string ProfileName;
            public UrlItem[] Urls;
        }

        [System.Serializable]
        public struct UrlItem //这儿的东西被编辑器下反射引用了
        {
            public NetworkMode NetworkMode;
            public string BaseUrl;
            public string HelloUrl;
        } 

        [System.Serializable]
        public enum NetworkMode
        {
            Normal          = 0,
            Editor          = 1,
            DevelopMode     = 2,
        }
    }
}
