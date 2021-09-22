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
using TinaX.VFS.Options;

namespace TinaX.Services
{
    public static class VFSServiceExtension
    {
        public static IXCore AddVFS(this IXCore core)
        {
            core.Services.AddOptions();
            core.Services.Configure<VFSOption>(options =>
            {
            });
            core.AddModule(new VFSModule());
            return core;
        }

        public static IXCore AddVFS(this IXCore core, Action<VFSOption> vfsOptions)
        {
            core.Services.AddOptions();
            core.Services.Configure<VFSOption>(vfsOptions);
            core.AddModule(new VFSModule());
            return core;
        }

        [Obsolete("Use \"AddVFS\"")]
        public static IXCore UseVFS(this IXCore core)
        {
            core.AddVFS();
            return core;
        }
    }
}
