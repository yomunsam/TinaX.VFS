namespace TinaX.VFSKit
{
    /// <summary>
    /// loaded asset . | 被加载出的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsset<T>
    {
        T Asset { get; }
    }
}
