using UObject = UnityEngine.Object;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset
{
    public interface ILoadAssetHandler
    {
        string HandlerName { get; }

        void LoadAsset(ref LoadAssetContext context, ref LoadAssetPayload payload);
    }
}
