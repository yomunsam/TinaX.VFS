namespace TinaX.VFS.Router
{
    /// <summary>
    /// 资产路由
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// 路由名称
        /// </summary>
        string RouterName { get; }
        bool TryQuery(string loadPath, out RoutingResult result);
    }
}
