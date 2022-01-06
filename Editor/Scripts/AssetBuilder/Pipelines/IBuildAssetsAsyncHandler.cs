using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines
{
    public interface IBuildAssetsAsyncHandler
    {
        string HandlerName { get; }

        Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken);
    }
}
