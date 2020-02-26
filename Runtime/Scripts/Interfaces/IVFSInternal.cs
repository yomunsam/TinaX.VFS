using System.Threading.Tasks;
using TinaX.VFSKit.Exceptions;

namespace TinaX.VFSKitInternal
{
    public interface IVFSInternal
    {
        Task<bool> Start();

        Task OnServiceClose();
        VFSException GetStartException();
    }
}

