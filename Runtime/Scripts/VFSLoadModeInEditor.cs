using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TinaX.VFSKitInternal
{
    public static class VFSLoadModeInEditor
    {

        static VFSLoadModeInEditor()
        {
//#if UNITY_EDITOR
//            ScriptableSingleton<VFSLoadModeInEditorCache>.instance.LoadMode = RuntimeAssetsLoadModeInEditor.LoadByAssetDatabase;
//#endif
        }

        public static RuntimeAssetsLoadModeInEditor GetLoadMode()
        {
#if UNITY_EDITOR
            return ScriptableSingleton<VFSLoadModeInEditorCache>.instance.LoadMode;
#else
            return RuntimeAssetsLoadModeInEditor.Normal;
#endif
        }

        /*
         * 注意啊，下面这些代码，仅可以在Editor下被调用
         * 之所以把#if UNITY_EDITOR 的宏包在方法里面，是为了防止一些自动生成代码的工具把这些方法生成进去，导致报错。（比如xlua）
         * 但是任何时候不要在Runtime下真正去使用这些方法。
         */
        public static string Get_Override_StreamingAssets_PackasgeRootFolderPath() 
        {
#if UNITY_EDITOR
            return ScriptableSingleton<VFSLoadModeInEditorCache>.instance.Override_StreamingAssets_PackagesRootFolderPath;
#else
            return null;
#endif
        }



    }

#if UNITY_EDITOR

    public class VFSLoadModeInEditorCache : ScriptableSingleton<VFSLoadModeInEditorCache>
    {
        //private RuntimeAssetsLoadModeInEditor _loadMode;
        public RuntimeAssetsLoadModeInEditor LoadMode = RuntimeAssetsLoadModeInEditor.LoadByAssetDatabase;

        public string Override_StreamingAssets_PackagesRootFolderPath;
    }

#endif


}
