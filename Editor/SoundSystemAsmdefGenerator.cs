using System.IO;
using UnityEditor;
using UnityEngine;

namespace Acfeel.SoundSystem.Editor
{
    public static class SoundSystemAsmdefGenerator
    {
        const string DefaultAsmdefName = "Acfeel.SoundSystem";
        const string DefaultEditorAsmdefName = "Acfeel.SoundSystem.Editor";
        const string DefaultPath = "Assets/Resources/SoundSystem/Acfeel.SoundSystem.asmdef";
        const string DefaultEditorPath = "Assets/Resources/SoundSystem/Acfeel.SoundSystem.Editor.asmdef";

        [MenuItem("Tools/SoundSystem/Create .asmdef files in Resources", priority = 1000)]
        public static void CreateAsmdefFiles()
        {
            bool overwrite = true;

            // 本体
            if (File.Exists(DefaultPath) || File.Exists(DefaultEditorPath))
            {
                overwrite = EditorUtility.DisplayDialog(
                    "Already Exists",
                    ".asmdef file(s) already exist.\nDo you want to overwrite them?",
                    "Yes", "No");
            }

            if (!overwrite) return;

            CreateAsmdefFile(DefaultPath, DefaultAsmdefName,
                references: new[] { "UniTask" }, includeEditor: false);
            CreateAsmdefFile(DefaultEditorPath, DefaultEditorAsmdefName,
                references: new[] { DefaultAsmdefName }, includeEditor: true);

            AssetDatabase.Refresh();
            Debug.Log($"Created or updated:\n- {DefaultPath}\n- {DefaultEditorPath}");
        }

        private static void CreateAsmdefFile(string path, string name, string[] references, bool includeEditor)
        {
            string refsJson = references != null && references.Length > 0
                ? $"[ \"{string.Join("\", \"", references)}\" ]"
                : "[]";

            string includePlatformsJson = includeEditor
                ? "[ \"Editor\" ]"
                : "[]";

            string json = @"{
    ""name"": """ + name + @""",
    ""references"": " + refsJson + @",
    ""includePlatforms"": " + includePlatformsJson + @",
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";

            File.WriteAllText(path, json);
        }
    }
}