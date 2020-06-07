using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatLib;

namespace TinaX.VFSKit
{
    public class VFS : Facade<IVFS>
    {
        public static IVFS Instance => VFS.That;
        public static IVFS I => VFS.That;

    }
}
