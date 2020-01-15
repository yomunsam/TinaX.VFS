using UnityEngine;

namespace TinaXEditor.VFSKit
{
    internal static class VFSConfigDashboardI18N
    {
        internal static string WindowTitle
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "VFS 面板";
                else
                    return "VFS Dashboard";
            }
        }

        internal static string GlobalVFS_Ignore_ExtName
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "忽略后缀名";
                else
                    return "Ignore extname";
            }
        }


        internal static string MsgBox_Common_Error
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "不太对劲";
                else
                    return "Oops!";
            }
        }

        internal static string MsgBox_Common_Confirm
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "好吧";
                else
                    return "Okey";
            }
        }

        internal static string MsgBox_Msg_CreateGroupNameIsNull
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "资源组名称无效哦.";
                else
                    return "The asset group name you want to create is not valid.";
            }
        }

        internal static string MsgBox_Msg_CreateGroupNameHasExists
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "要创建的资源组的名称\"{0}\"已经存在咯.";
                else
                    return "The name \"{0}\" of the assets group you want to create already exists.";
            }
        }

        internal static string Groups_Item_Null_Tips
        {
            get
            {
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    return "当前配置中没有任何资源组信息，请在窗口上方工具栏新建资源组。";
                else
                    return "There is no assets group information in the current configuration. Please create a new in the toolbar above the window.";
            }
        }

    }
}