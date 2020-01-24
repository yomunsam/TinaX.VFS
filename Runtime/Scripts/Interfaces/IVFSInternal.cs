using System.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    public interface IVFSInternal
    {
        Task<bool> Start();

        Task OnServiceClose();
    }
}

