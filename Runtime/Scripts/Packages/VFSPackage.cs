/*
 * This file is part of the "TinaX Framework".
 * https://github.com/yomunsam/TinaX
 *
 * (c) Nekonya Studio <me@yomunchan.moe> <yomunsam@nekonya.io>
 * https://nekonya.io
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

using System.Collections.Generic;
using TinaX.VFS.Groups;

namespace TinaX.VFS.Packages
{
    /*
     * VFS Package是对VFS 6.x中Group概念的进一步梳理。
     */

    /// <summary>
    /// VFS Package
    /// </summary>
    public class VFSPackage
    {
        public VFSPackage() { }

        public List<VFSGroup> Groups { get; private set; } = new List<VFSGroup>();

    }
}
