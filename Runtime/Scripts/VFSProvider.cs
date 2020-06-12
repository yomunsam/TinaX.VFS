using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.Services;
using TinaX.VFSKit.Const;
using TinaX.VFSKitInternal;

namespace TinaX.VFSKit
{
    [XServiceProviderOrder(60)]
    public class VFSProvider : IXServiceProvider
    {
        public string ServiceName => VFSConst.ServiceName;

        public Task<XException> OnInit(IXCore core) => Task.FromResult<XException>(null);

        public void OnServiceRegister(IXCore core)
        {
            core.Services.BindBuiltInService<IAssetService, IVFS, VFSKit>()
                .SetAlias<IVFSInternal>();
        }
        
        public Task<XException> OnStart(IXCore core)
            => core.Services.Get<IVFSInternal>().Start();

        public void OnQuit() { }

        public Task OnRestart() => Task.CompletedTask;
    }
}
