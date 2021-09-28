namespace TinaX.VFS.ConfigProviders
{
    /// <summary>
    /// 配置错误
    /// </summary>
    public struct ConfigError
    {
        public ConfigErrorType ErrorType { get; set; }
        public string ErrorTypeName { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误来源名称
        /// </summary>
        public string ErrorSourceName { get; set; }
    }

    public enum ConfigErrorType : int
    {
        Unknow          = 0,
    }
}
