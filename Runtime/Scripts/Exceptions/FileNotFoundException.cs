using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit.Exceptions
{
    public class FileNotFoundException : VFSException
    {
        public string Path { get; private set; }

        public FileNotFoundException(string message,string filePath) : base(message, VFSErrorCode.FileNotFound) { this.Path = filePath; }


    }
}
