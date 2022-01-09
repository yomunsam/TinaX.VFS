namespace TinaX.VFS.Interfaces
{
    /// <summary>
    /// 引用计数接口
    /// </summary>
    public interface IRefCounter
    {
        int RefCount { get; }

        /// <summary>
        /// +1s
        /// </summary>
        void Retain();
        void Release();
    }
}
