using UnityEditor;

namespace Acfeel.SoundSystem.Editor
{
    public static class SoundSystemSetupMenu
    {
        [MenuItem("Tools/SoundSystem/Create Settings Asset")]
        public static void CreateSettingsAssetManually()
        {
            SoundSystemSettingsAutoCreator.CreateSettingsAsset();
        }
    }
}