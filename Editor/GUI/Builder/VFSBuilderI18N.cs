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

        internal static string ClearAllABSignBeforeStart
        {
            get
            {
                if (IsChinese) return "在开始前清理所有AssetBundle标记：";
                return "Clear all Assetbundle signs before start : ";
            }
        }
        
        internal static string ClearAllABSignBeforeStart_Tips
        {
            get
            {
                if (IsChinese) return "防止项目开发过程中从外部导入了包含AssetBundle配置信息但实际上不需要打包的冗余资源。";
                return "Prevent redundant resources that contain AssetBundle configuration information but do not need to be packaged from outside during project development.";
            }
        }

        internal static string ClearAllABSignAfterFinish
        {
            get
            {
                if (IsChinese) return "在资源构建结束后清理全部AssetBundle标记：";
                return "Clear all Assetbundle signs after build finish : ";
            }
        }
        
        internal static string ClearOutputFolders
        {
            get
            {
                if (IsChinese) return "清空资源输出目录：";
                return "Clear Output Folder: ";
            }
        }
        
        internal static string ForceRebuild
        {
            get
            {
                if (IsChinese) return "强制重构建资源：";
                return "Force Rebuild Assets: ";
            }
        }
        
        internal static string SwitchProfile
        {
            get
            {
                if (IsChinese) return "切换";
                return "Switch ";
            }
        }
    }
}
