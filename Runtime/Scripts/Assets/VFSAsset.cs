using System;
using TinaX.VFS.Interfaces;

#nullable enable
namespace TinaX.VFS.Assets
{
    public class VFSAsset : IAsset, IRefCounter
    {
        public UnityEngine.Object Asset => throw new NotImplementedException();

        public int RefCount => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TAsset Get<TAsset>() where TAsset : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void Release()
        {
            throw new NotImplementedException();
        }

        public void Retain()
        {
            throw new NotImplementedException();
        }
    }

    public class VFSAsset<TAsset> : VFSAsset, IAsset<TAsset> where TAsset : UnityEngine.Object
    {
        TAsset IAsset<TAsset>.Asset => throw new NotImplementedException();

        public TAsset Get()
        {
            throw new NotImplementedException();
        }
    }
}
