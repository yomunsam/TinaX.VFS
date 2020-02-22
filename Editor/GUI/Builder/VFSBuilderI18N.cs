using UnityEngine;

namespace TinaXEditor.VFSKitInternal.I18N
{ 
    internal static class VFSBuilderI18N
    {
        private static bool? _isChinese;
        private static bool IsChinese
        {
            get
            {
                if(_isChinese == null)
                {
                    _isChinese = (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified);
                }
                return _isChinese.Value;
            }
        }


        internal static string WindowTitle
        {
            get
            {
                return "VFS Builder";
            }
        }

        internal static string PlatformTarget
        {
            get
            {
                if (IsChinese) return "平台目标：";
                return "Platform Target : ";
            }
        }
        
        internal static string strictMode
        {
            get
            {
                if (IsChinese) return "严格模式：";
                return "Strict Mode : ";
            }
        }

        internal static string AssetCompressType
        {
            get
            {
                if (IsChinese) return "资源压缩模式：";
                return "Compress Type : ";
            }
        }
        
        internal static string CopyToStramingAssetPath
        {
            get
            {
                if (IsChinese) return "复制到StreamingAssers目录：";
                return "Copy To \"StreamingAssets\": ";
            }
        }

    }
}
