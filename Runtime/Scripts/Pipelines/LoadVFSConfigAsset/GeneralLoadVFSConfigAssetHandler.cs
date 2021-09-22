using System.Threading;
using Cysharp.Threading.Tasks;

namespace TinaX.VFS.Pipelines.LoadVFSConfigAsset
{
    public class GeneralLoadVFSConfigAssetHandler : ILoadVFSConfigAssetHandler
    {
        public delegate UniTask LoadVFSConfigAssetAsyncDelegate(LoadVFSConfigAssetContext context, ILoadVFSConfigAssetHandler next, CancellationToken cancellationToken);

        public GeneralLoadVFSConfigAssetHandler(string name, LoadVFSConfigAssetAsyncDelegate func)
        {
            HandlerName = name;
            LoadVFSConfigAssetFunc = func;
        }

        public string HandlerName { get; private set; }
        public LoadVFSConfigAssetAsyncDelegate LoadVFSConfigAssetFunc { get; private set; }

        public UniTask LoadVFSConfigAssetAsync(LoadVFSConfigAssetContext context, ILoadVFSConfigAssetHandler next, CancellationToken cancellationToken)
        {
            return LoadVFSConfigAssetFunc(context, next, cancellationToken);
        }
    }
}
