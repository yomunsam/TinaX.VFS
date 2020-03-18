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

        public Task<bool> OnInit()
        {
            return Task.FromResult(true);
        }

        public XException GetInitException()
        {
            return null;
        }

        public void OnServiceRegister()
        {
            XCore.GetMainInstance()?.BindSingletonService<IVFS, IAssetService, VFSKit>().SetAlias<IVFSInternal>();
        }

        public Task<bool> OnStart()
        {
            return XCore.GetMainInstance()?.GetService<IVFSInternal>().Start();
        }

        public XException GetStartException()
        {
            return XCore.GetMainInstance()?.GetService<IVFSInternal>().GetStartException();
        }

        public void OnQuit()
        {
            _ = XCore.GetMainInstance()?.GetService<IVFSInternal>().OnServiceClose();
        }

        

        public Task OnRestart()
        {
            return XCore.GetMainInstance()?.GetService<IVFSInternal>().OnServiceClose();
        }

        

        
    }
}
