using System.Threading;
using CatLib.Container;
using Cysharp.Threading.Tasks;
using TinaX.Container;
using TinaX.Core.Behaviours;
using TinaX.Module;
using TinaX.Modules;
using TinaX.Options;
using TinaX.Services;
using TinaX.VFS.Consts;
using TinaX.VFS.Internal;
using TinaX.VFS.Options;
using TinaX.VFS.Services;
using UnityEngine;

namespace TinaX.VFS
{
    [ModuleProviderOrder(80)]
    public class VFSModule : IModuleProvider
    {
        public string ModuleName => VFSConsts.ServiceName;


        public UniTask<ModuleBehaviourResult> OnInitAsync(IServiceContainer services, CancellationToken cancellationToken)
            => UniTask.FromResult(ModuleBehaviourResult.CreateSuccess(ModuleName));

        public void ConfigureServices(IServiceContainer services)
        {
            var bindData = services.Singleton<IVFS, VFSService>().Alias<IVFSInternal>();
            var options = services.Get<IOptions<VFSOption>>();
            if (options.Value.ImplementBuiltInAssetServiceInterface)
            {
                bindData.Alias<IAssetService>();
            }
        }

        public void ConfigureBehaviours(IBehaviourManager behaviour, IServiceContainer services) { }

        public async UniTask<ModuleBehaviourResult> OnStartAsync(IServiceContainer services, CancellationToken cancellationToken)
        {
#if TINAX_DEV
            Debug.Log("VFS Module 开始启动");
#endif
            await services.Get<IVFSInternal>().StartAsync(cancellationToken);
            return ModuleBehaviourResult.CreateSuccess(ModuleName);
        }


        

        public UniTask OnRestartAsync(IServiceContainer services, CancellationToken cancellationToken)
            => UniTask.CompletedTask;

        public void OnQuit()
        {
        }

    }
}
