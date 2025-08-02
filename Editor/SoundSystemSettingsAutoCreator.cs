using UnityEditor;
using UnityEngine;
using Acfeel.SoundSystem;

namespace Acfeel.SoundSystem.Editor
{
    [InitializeOnLoad]
    public static class SoundSystemSettingsAutoCreator
    {
        static SoundSystemSettingsAutoCreator()
        {
            const string path = "SoundSystemSettings";
            var settings = Resources.Load<SoundSystemSettings>(path);
            if (settings == null)
            {
                CreateSettingsAsset();
            }
        }

        private static void CreateSettingsAsset()
        {
            var instance = ScriptableObject.CreateInstance<SoundSystemSettings>();
            const string folder = "Assets/Resources";
            const string assetPath = folder + "/SoundSystemSettings.asset";

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Resources", "SoundSystem");

            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("SoundSystemSettings.asset を自動生成しました。");
        }
    }
}