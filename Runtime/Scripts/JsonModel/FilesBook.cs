using System;

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
