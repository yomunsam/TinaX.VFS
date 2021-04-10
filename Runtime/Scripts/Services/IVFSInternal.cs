using System.Threading.Tasks;

namespace TinaX.VFSKit.Internal
{
    public interface IVFSInternal
    {
        Task<XException> StartAsync();
    }
}
