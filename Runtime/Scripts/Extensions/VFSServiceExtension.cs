using System;
using TinaX.Options;
using TinaX.VFSKit;

namespace TinaX.Services
{
    public static class VFSServiceExtension
    {
        public static IXCore UseVFS(this IXCore core, Action<VFSOption> options = null)
        {
            if(options == null)
            {
                options = new Action<VFSOption>(opt =>
                {

                });
            }
            core.Services.Configure<VFSOption>(options);
            core.RegisterServiceProvider(new VFSProvider());
            return core;
        }

        
    }
}
