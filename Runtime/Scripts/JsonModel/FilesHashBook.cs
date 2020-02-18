using System.Collections;

namespace TinaX.VFSKitInternal
{
    [System.Serializable]
    public class FilesHashBook
    {
        public FileHash[] Files;

        private Hashtable _dict_files;
        private Hashtable dict_files
        {
            get
            {
                if(_dict_files == null)
                {
                    lock (this)
                    {
                        if(_dict_files == null)
                        {
                            _dict_files = new Hashtable();
                            if (Files != null)
                            {
                                foreach(var item in Files)
                                {
                                    _dict_files.Add(item.p, item.h);
                                }
                            }
                        }
                    }
                }
                return _dict_files;
            }
        }

        public bool TryGetFileHashValue(string filePath)
        {
            if (dict_files.ContainsKey(filePath))
            {
                filePath = (string)dict_files[filePath];
                return true;
            }
            else
            {
                filePath = string.Empty;
                return false;
            }
        }


        [System.Serializable]
        public struct FileHash
        {
            /// <summary>
            /// File Path
            /// </summary>
            public string p;

            /// <summary>
            /// h
            /// </summary>
            public string h;
        }
    }
}

