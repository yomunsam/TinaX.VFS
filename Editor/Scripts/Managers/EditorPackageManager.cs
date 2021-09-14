using TinaX.VFS.Managers;
using TinaX.VFS.Options;

namespace TinaXEditor.VFS.Managers
{
    public class EditorPackageManager : PackageManager
    {
        public EditorPackageManager() : base() { }

        public EditorPackageManager(VFSPackageOption mainPackageOption) : base(mainPackageOption)
        {

        }
    }
}
