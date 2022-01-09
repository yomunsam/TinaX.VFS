using Cysharp.Threading.Tasks;
using UObject = UnityEngine.Object;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset
{
    public interface ILoadAssetAsyncHandler
    {
        string HandlerName { get; }

        UniTask LoadAssetAsync(LoadAssetContext context, LoadAssetPayload payload);
    }
}
