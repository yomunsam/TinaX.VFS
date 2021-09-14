using System;
using UObject = UnityEngine.Object;

namespace TinaX.VFS
{
    public interface IAsset : IDisposable
    {
        T Get<T>() where T : UObject;
        UObject Get();
    }
}
