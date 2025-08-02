#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace Acfeel.SoundSystem.Editor
{
    [CustomEditor(typeof(SoundSystemSettings))]
    public class SoundSystemSettingsEditor : UnityEditor.Editor
    {
        private const string Symbol = "SOUNDSYSTEM_ADDRESSABLES_SUPPORT";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Addressables サポート設定", EditorStyles.boldLabel);

            bool isDefined = IsDefineSymbolSet(Symbol);

            if (isDefined)
            {
                EditorGUILayout.HelpBox($"ScriptingDefineSymbolsに {Symbol} は定義されています。", MessageType.Info);

                if (GUILayout.Button($"{Symbol} を削除"))
                {
                    if (EditorUtility.DisplayDialog("確認", $"{Symbol} を削除してもよろしいですか？", "削除", "キャンセル"))
                    {
                        RemoveDefineSymbol(Symbol);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"ScriptingDefineSymbolsに {Symbol} は未定義です。", MessageType.Warning);

                if (GUILayout.Button($"{Symbol} を追加"))
                {
                    AddDefineSymbol(Symbol);
                }
            }
        }

        private NamedBuildTarget CurrentBuildTarget =>
            NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        private bool IsDefineSymbolSet(string symbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbols(CurrentBuildTarget);
            return symbols.Split(';').Any(s => s.Trim() == symbol);
        }

        private void AddDefineSymbol(string symbol)
        {
            var buildTarget = CurrentBuildTarget;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            if (!symbols.Split(';').Contains(symbol))
            {
                string updated = string.IsNullOrEmpty(symbols) ? symbol : symbols + ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, updated);
                Debug.Log($"定義シンボル \"{symbol}\" を追加しました。");
            }
        }

        private void RemoveDefineSymbol(string symbol)
        {
            var buildTarget = CurrentBuildTarget;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            string[] list = symbols.Split(';');
            string updated = string.Join(";", list.Where(s => s.Trim() != symbol));
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, updated);
            Debug.Log($"定義シンボル \"{symbol}\" を削除しました。");
        }
    }
}
#endif