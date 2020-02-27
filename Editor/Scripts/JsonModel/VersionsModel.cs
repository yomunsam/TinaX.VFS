using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit.Versions
{
    [Serializable]
    public class VersionsModel
    {
        public string[] branches; //只存储了branch名称


        private List<string> _branches;
        private List<string> list_branches
        {
            get
            {
                if (_branches == null)
                {
                    _branches = new List<string>();
                    if (branches != null) _branches.AddRange(branches);

                }
                return _branches;
            }
        }

        public List<string> Branches_ReadWrite => list_branches;

        public void ReadySave()
        {
            branches = list_branches.ToArray();
        }


    }
}
