using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TinaX.VFSKit
{
    public interface IAsset : IDisposable
    {
        Object Asset { get; }

        T Get<T>() where T : Object;
        Object Get();
        void Release();
    }


}
