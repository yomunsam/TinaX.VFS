using System;
using UObject = UnityEngine.Object;

namespace TinaX.VFSKit
{
    public interface IAsset : IDisposable
    {
        T Get<T>() where T : UObject;
        UObject Get();
    }
}
