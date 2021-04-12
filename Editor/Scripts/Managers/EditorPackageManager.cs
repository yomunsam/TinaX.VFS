using TinaX.VFSKit.Managers;
using TinaX.VFSKit.Options;

namespace TinaXEditor.VFSKit.Managers
{
    public class EditorPackageManager : PackageManager
    {
        public EditorPackageManager() : base() { }

        public EditorPackageManager(VFSPackageOption mainPackageOption) : base(mainPackageOption)
        {

        }
    }
}
