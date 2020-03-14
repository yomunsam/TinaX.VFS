using System;
using System.Collections.Generic;

namespace TinaX.VFSKitInternal
{
    [Serializable]
    public class PatchRecords
    {
        public string[] Add;
        public string[] Modify;
        public string[] Delete;


        private List<string> _add_list;
        private List<string> _modify_list;
        private List<string> _delete_list;
        public List<string> Add_ReadWrite
        {
            get
            {
                if(_add_list == null)
                {
                    _add_list = new List<string>();
                    if (this.Add != null)
                        _add_list.AddRange(this.Add);
                }
                return _add_list;
            }
        }

        public List<string> Modify_ReadWrite
        {
            get
            {
                if (_modify_list == null)
                {
                    _modify_list = new List<string>();
                    if (this.Modify != null)
                        _modify_list.AddRange(this.Modify);
                }
                return _modify_list;
            }
        }

        public List<string> Delete_ReadWrite
        {
            get
            {
                if (_delete_list == null)
                {
                    _delete_list = new List<string>();
                    if (this.Delete != null)
                        _delete_list.AddRange(this.Delete);
                }
                return _delete_list;
            }
        }

        public void SaveReady()
        {
            this.Add = this.Add_ReadWrite.ToArray();
            this.Modify = this.Modify_ReadWrite.ToArray();
            this.Delete = this.Delete_ReadWrite.ToArray();
        }

    }

}
