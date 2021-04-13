using UnityEditor;

namespace TinaXEditor.VFSKit.ProjectSettings.Internal
{
    public static class VFSProjectSetting
    {
        [SettingsProvider]
        public static SettingsProvider GetSetting()
        {
            return new SettingsProvider()
        }
    }
}
