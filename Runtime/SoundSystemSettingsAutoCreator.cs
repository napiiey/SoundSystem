#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Acfeel.SoundSystem.Editor
{
    [InitializeOnLoad]
    public static class SoundSystemSettingsAutoCreator
    {
        static SoundSystemSettingsAutoCreator()
        {
            const string resourcePath = "SoundSystem/SoundSystemSettings";
            var settings = Resources.Load<SoundSystemSettings>(resourcePath);
            if (settings == null)
            {
                CreateSettingsAsset();
            }
        }

        public static void CreateSettingsAsset()
        {
            var instance = ScriptableObject.CreateInstance<SoundSystemSettings>();

            const string baseFolder = "Assets/Resources";
            const string soundSystemFolder = baseFolder + "/SoundSystem";
            const string assetPath = soundSystemFolder + "/SoundSystemSettings.asset";

            // Ensure base folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Ensure SoundSystem folder exists
            if (!AssetDatabase.IsValidFolder(soundSystemFolder))
            {
                AssetDatabase.CreateFolder(baseFolder, "SoundSystem");
            }

            // Create subfolders: Bgm, Amb, Se
            CreateSubFolder(soundSystemFolder, "Bgm");
            CreateSubFolder(soundSystemFolder, "Amb");
            CreateSubFolder(soundSystemFolder, "Se");

            // Create the ScriptableObject asset
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("SoundSystemSettings.asset created at: " + assetPath);
        }

        static void CreateSubFolder(string parent, string child)
        {
            string childPath = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(childPath))
            {
                AssetDatabase.CreateFolder(parent, child);
                Debug.Log("Created folder: " + childPath);
            }
        }
    }
}
#endif
