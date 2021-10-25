using System.Threading;
using Cysharp.Threading.Tasks;

namespace TinaX.VFS.Internal
{
    public interface IVFSInternal
    {
        UniTask StartAsync(CancellationToken cancellationToken);
    }
}