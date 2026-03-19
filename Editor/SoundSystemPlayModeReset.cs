using UnityEditor;
using UnityEngine;

namespace Acfeel.SoundSystem.Editor
{
    [InitializeOnLoad]
    static class SoundSystemPlayModeReset
    {
        static SoundSystemPlayModeReset()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.ExitingEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                SingletonMonoBehaviour<SoundSystem>.ResetSingletonState();
            }
        }
    }
}
