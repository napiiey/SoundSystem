#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Acfeel.SoundSystem.Editor
{
    public static class SoundSystemAsmdefGenerator
    {
        const string DefaultAsmdefName = "Acfeel.SoundSystem";
        const string DefaultPath = "Assets/Acfeel.SoundSystem.asmdef";

        [MenuItem("Tools/SoundSystem/Create .asmdef in Assets", priority = 1000)]
        public static void CreateAsmdef()
        {
            if (File.Exists(DefaultPath))
            {
                if (!EditorUtility.DisplayDialog("Already Exists",
                        ".asmdef already exists in Assets.\nDo you want to overwrite it?", "Yes", "No"))
                {
                    return;
                }
            }

            string json = @"{
    ""name"": """ + DefaultAsmdefName + @""",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";

            File.WriteAllText(DefaultPath, json);
            AssetDatabase.Refresh();

            Debug.Log($"Created {DefaultPath}");
        }
    }
}
#endif