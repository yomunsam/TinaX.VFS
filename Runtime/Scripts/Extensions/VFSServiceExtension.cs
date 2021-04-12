/*
 * This file is part of the "TinaX Framework VFS".
 * https://github.com/yomunsam/TinaX.VFS
 *
 * (c) Nekonya Studio <me@yomunchan.moe> <yomunsam@nekonya.io>
 * https://nekonya.io
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

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
