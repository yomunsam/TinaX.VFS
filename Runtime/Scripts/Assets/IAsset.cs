using System;
using UObject = UnityEngine.Object;

namespace TinaX.VFS
{
    public interface IAsset : IDisposable
    {
        UObject Asset { get; }
        TAsset Get<TAsset>() where TAsset : UObject;
    }

    public interface IAsset<TAsset> : IAsset where TAsset : UObject
    {
        new TAsset Asset { get; }
        TAsset Get();
    }

}
