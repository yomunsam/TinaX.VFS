using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX.VFSKit.Exceptions;

namespace TinaX.VFSKitInternal
{
    public interface IVFSInternal
    {
        Task<XException> Start();

        List<VFSBundle> GetAllBundle();
        bool LoadFromAssetbundle();
#if UNITY_EDITOR
        List<EditorAsset> GetAllEditorAsset();
#endif

    }
}

