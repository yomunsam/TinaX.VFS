using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit
{
    /// <summary>
    /// 在编辑器下运行游戏的时候，Runtime怎么加载资源
    /// </summary>
    public enum RuntimeAssetsLoadModeInEditor
    {
        Normal                          = 0,     //正常加载
        Override_StreamingAssetsPath    = 1,   //覆盖原有StreamingAssets
        LoadByAssetDatabase             = 2, //自己在编辑器下加载
    }
}
