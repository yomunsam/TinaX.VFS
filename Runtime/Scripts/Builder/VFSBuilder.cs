using System;
using System.ComponentModel;
using TinaX.Container;
using TinaX.Options;
using TinaX.VFS.Options;

#nullable enable
namespace TinaX.VFS.Builder
{
    public class VFSBuilder
    {

        public VFSBuilder(IServiceContainer services)
        {
            this.Services = services;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly IServiceContainer Services;

        public VFSBuilder Configure(Action<VFSOptions> configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.Services.AddOptions();
            this.Services.Configure<VFSOptions>(configuration);
            return this;
        }

    }
}
