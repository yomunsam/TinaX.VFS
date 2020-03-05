using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit
{
    public interface IRefCounter
    {
        int RefCount { get; }

        /// <summary>
        /// +1s
        /// </summary>
        void Retain();
        void Release();

    }
}
