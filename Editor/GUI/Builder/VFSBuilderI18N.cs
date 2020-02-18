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

    }
}
