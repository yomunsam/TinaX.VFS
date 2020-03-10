using UnityEngine;

namespace TinaX.VFSKit
{
    public interface IAsset
    {
        Object Asset { get; }

        T Get<T>() where T : Object;
        Object Get();
        void Release();
    }


}
