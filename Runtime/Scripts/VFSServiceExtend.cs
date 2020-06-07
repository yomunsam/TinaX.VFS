using TinaX.VFSKit;

namespace TinaX.Services
{
    public static class VFSServiceExtend
    {
        public static IXCore UseVFS(this IXCore core)
        {
            core.RegisterServiceProvider(new VFSProvider());
            return core;
        }
    }
}
