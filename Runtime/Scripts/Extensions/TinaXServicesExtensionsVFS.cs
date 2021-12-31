/*
 * This file is part of the "TinaX Framework VFS".
 * https://github.com/yomunsam/TinaX.VFS
 *
 * (c) Nekonya Studio <me@yomunchan.moe> <yomunsam@gmail.com>
 * https://nekonya.io
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

using System;
using TinaX.Options;
using TinaX.VFS;
using TinaX.VFS.Builder;
using TinaX.VFS.Options;

namespace TinaX.Services
{
    public static class TinaXServicesExtensionsVFS
    {
        public static IXCore AddVFS(this IXCore core, Action<VFSBuilder> vfsBuilder)
        {
            //---------------------------------------------------------------------------------
            //因为这个Builder不复杂，所有我们就直接在这儿顺便实现builder模式里的Director，不另外写个class了

            var builder = new VFSBuilder(core.Services);
            vfsBuilder?.Invoke(builder);

            //Options
            if (!core.Services.CanGet<IOptions<VFSOptions>>())
            {
                core.Services.AddOptions();
                core.Services.Configure<VFSOptions>(options => { });
            }


            //--------------------------------------------------------------------------------------
            core.AddModule(new VFSModule());

            return core;
        }
    }
}
