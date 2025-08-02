using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Acfeel.SoundSystem.Editor
{
    [CustomEditor(typeof(SoundSystemSettings))]
    public class SoundSystemSettingsEditor : UnityEditor.Editor
    {
        const string Symbol = "SOUNDSYSTEM_ADDRESSABLES_SUPPORT";

        NamedBuildTarget CurrentBuildTarget =>
            NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Addressables Support", EditorStyles.boldLabel);

            bool isDefined = IsDefineSymbolSet(Symbol);

            if (isDefined)
            {
                EditorGUILayout.HelpBox($"Enabled.\n \"{Symbol}\" is defined in Scripting Define Symbols.",
                    MessageType.Info);

                if (GUILayout.Button($"Remove \"{Symbol}\""))
                {
                    if (EditorUtility.DisplayDialog("Confirmation", $"Are you sure you want to remove \"{Symbol}\"?",
                            "Remove", "Cancel"))
                    {
                        RemoveDefineSymbol(Symbol);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"Disabled.\n \"{Symbol}\" is not defined in Scripting Define Symbols.",
                    MessageType.Info);

                if (GUILayout.Button($"Add \"{Symbol}\""))
                {
                    AddDefineSymbol(Symbol);
                }
            }
        }

        bool IsDefineSymbolSet(string symbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbols(CurrentBuildTarget);
            return symbols.Split(';').Any(s => s.Trim() == symbol);
        }

        void AddDefineSymbol(string symbol)
        {
            NamedBuildTarget buildTarget = CurrentBuildTarget;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            if (!symbols.Split(';').Contains(symbol))
            {
                string updated = string.IsNullOrEmpty(symbols) ? symbol : symbols + ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, updated);
                Debug.Log($"Added define symbol \"{symbol}\".");
            }
        }

        void RemoveDefineSymbol(string symbol)
        {
            NamedBuildTarget buildTarget = CurrentBuildTarget;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            string[] list = symbols.Split(';');
            string updated = string.Join(";", list.Where(s => s.Trim() != symbol));
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, updated);
            Debug.Log($"Removed define symbol \"{symbol}\".");
        }
    }
}