using TinaX;

namespace TinaX.VFSKit.Exceptions
{
    public class VFSException : XException
    {
        public VFSException(string message) : base("[TinaX.VFS]" + message) { base.ServiceException = true; base.ServiceName = TinaX.VFSKit.Const.VFSConfig.ServiceName; }
        
        public VFSException(string message, int errorCode): base("[TinaX.VFS]" + message, errorCode) { base.ServiceException = true; base.ServiceName = TinaX.VFSKit.Const.VFSConfig.ServiceName; }
        public VFSException(string message, VFSErrorCode errorCode): base("[TinaX.VFS]" + message, (int)errorCode) { base.ServiceException = true; base.ServiceName = TinaX.VFSKit.Const.VFSConfig.ServiceName; }

    
    }

    /*
     * VFS 错误码
     * 
     * 【基础错误】
     * - 1          -> 未成功加载到配置文件
     * 
     * 【配置文件错误】
     * - 200        -> 资源组规则冲突
     * 
     * 
     * 
     */


}

