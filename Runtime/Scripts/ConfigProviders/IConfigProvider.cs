namespace TinaX.VFS.ConfigProviders
{
    /// <summary>
    /// 各级配置提供者的基础接口
    /// </summary>
    public interface IConfigProvider<T>
    {
        /// <summary>
        /// 已标准化了吗
        /// </summary>
        bool Standardized { get; }

        /// <summary>
        /// 已完成错误检查了吗
        /// </summary>
        bool CheckCompleted { get; }

        /// <summary>
        /// 获取我们要的配置
        /// </summary>
        T Configuration { get; }

        /// <summary>
        /// 对配置进行标准化
        /// </summary>
        void Standardize();

        /// <summary>
        /// 错误检查
        /// </summary>
        /// <param name="error">如果有错误，返回第一个检查到的错误结果</param>
        /// <returns>如果有错误则返回true</returns>
        bool TryCheckError(out ConfigError? error);
        
    }
}
