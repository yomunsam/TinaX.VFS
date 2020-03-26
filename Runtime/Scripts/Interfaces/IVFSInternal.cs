using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX.VFSKit.Exceptions;

namespace TinaX.VFSKitInternal
{
    public interface IVFSInternal
    {
        Task<bool> Start();

        Task OnServiceClose();
        VFSException GetStartException();
        List<VFSBundle> GetAllBundle();
        bool LoadFromAssetbundle();
#if UNITY_EDITOR
        List<EditorAsset> GetAllEditorAsset();
#endif

    }
}

