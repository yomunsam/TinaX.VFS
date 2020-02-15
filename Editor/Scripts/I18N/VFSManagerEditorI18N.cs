using System;
using UnityEngine;

namespace TinaXEditor.VFSKitInternal.I18N
{

    internal static class VFSManagerEditorI18N
    {
        //cached
        private static bool Ischinese = (Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.Chinese);

        internal static string Log_ConfigureGroupsConflict
        {
            get
            {
                if (Ischinese)
                {
                    return "[TinaX VFS] VFS配置文件中“资源组”的配置规则冲突。";
                }
                return "[TinaX VFS] Configuration rule conflict for \"Group\" in VFS configuration file.";
            }   
        }

        internal static string Log_SameGroupName
        {
            get
            {
                if (Ischinese)
                {
                    return "[TinaX VFS] VFS配置文件中存在相同的Group名，请修复。";
                }
                return "[TinaX VFS] The same group name exists in the VFS configuration file.";
            }
        }
    }
}
