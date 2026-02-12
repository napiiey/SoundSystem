using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Acfeel.SoundSystem.Editor
{
    /// <summary>
    /// Addressablesパッケージの有無に応じて、SoundSystemの.asmdefファイルの参照を自動的に更新するスクリプト。
    /// </summary>
    [InitializeOnLoad]
    public static class SoundSystemAsmdefUpdater
    {
        private const string AsmdefGuid = "741f2382cf8e84e49989396beff430cc"; // Acfeel.SoundSystem.asmdef のデフォルトの場所を想定（GUIDで追跡可能なら理想的ですが、パス指定の方が確実な場合もあります）
        private const string AsmdefName = "Acfeel.SoundSystem";
        
        private static readonly string[] RequiredAddressablesReferences = new[]
        {
            "Unity.Addressables",
            "Unity.ResourceManager"
        };

        static SoundSystemAsmdefUpdater()
        {
            UpdateAsmdefReferences();
        }

        [MenuItem("Tools/SoundSystem/Force Update Asmdef References")]
        public static void UpdateAsmdefReferences()
        {
            string asmdefPath = GetAsmdefPath();
            if (string.IsNullOrEmpty(asmdefPath))
            {
                // Debug.LogWarning("[SoundSystem] Could not find Acfeel.SoundSystem.asmdef. Skipping auto-update.");
                return;
            }

            bool hasAddressables = HasAddressablesPackage();
            string json = File.ReadAllText(asmdefPath);
            AsmdefData data = JsonUtility.FromJson<AsmdefData>(json);

            if (data.references == null) data.references = new List<string>();

            bool changed = false;

            if (hasAddressables)
            {
                foreach (var reqRef in RequiredAddressablesReferences)
                {
                    if (!data.references.Contains(reqRef))
                    {
                        data.references.Add(reqRef);
                        changed = true;
                    }
                }
            }
            else
            {
                foreach (var reqRef in RequiredAddressablesReferences)
                {
                    if (data.references.Contains(reqRef))
                    {
                        data.references.Remove(reqRef);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                // JsonUtilityは整形が甘いので、手動で少し整形するか、そのまま書き込みます。
                // Unityの.asmdefは標準的なJSONなのでJsonUtilityで書き戻せますが、
                // 他のフィールド（versionDefinesなど）が消えないように注意が必要です。
                // JsonUtilityは未知のフィールドを無視するため、完全なデータ構造を定義する必要があります。
                
                string newJson = JsonUtility.ToJson(data, true);
                File.WriteAllText(asmdefPath, newJson);
                AssetDatabase.ImportAsset(asmdefPath);
                Debug.Log($"[SoundSystem] Updated {AsmdefName}.asmdef references. Addressables Support: {hasAddressables}");
            }
        }

        private static string GetAsmdefPath()
        {
            // 名前で検索
            string[] guids = AssetDatabase.FindAssets($"{AsmdefName} t:Asmdef");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == AsmdefName)
                {
                    return path;
                }
            }
            return null;
        }

        private static bool HasAddressablesPackage()
        {
            // Addressablesのアセンブリが定義されているか、またはパッケージが存在するかを確認
#if UNITY_2019_3_OR_NEWER
            // パッケージマネージャー経由での確認が確実ですが、同期的に行うには Type.GetType やアセンブリ走査が簡単です。
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Unity.Addressables");
#else
            return false;
#endif
        }

        [Serializable]
        private class AsmdefData
        {
            public string name;
            public string rootNamespace;
            public List<string> references;
            public List<string> includePlatforms;
            public List<string> excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public List<string> precompiledReferences;
            public bool autoReferenced;
            public List<string> defineConstraints;
            public List<VersionDefine> versionDefines;
            public bool noEngineReferences;
        }

        [Serializable]
        private class VersionDefine
        {
            public string name;
            public string expression;
            public string define;
        }
    }
}
