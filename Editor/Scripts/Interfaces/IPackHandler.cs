using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinaXEditor.VFSKit
{
    /// <summary>
    /// 打包管线 处理器
    /// </summary>
    public interface IPackHandler
    {
        void OnBundlePackaged(string path);
    }
}

