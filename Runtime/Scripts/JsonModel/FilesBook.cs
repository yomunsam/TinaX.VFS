using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    [Serializable]
    public class FilesBook
    {
        public string[] Files;

        public FilesBook() { }

        public FilesBook(string[] files)
        {
            Files = files;
        }
    }
}
