using UnityEngine;

namespace Acfeel.SoundSystem
{
    [CreateAssetMenu(fileName = "SoundSystemSettings", menuName = "SoundSystem/Create Settings")]
    public class SoundSystemSettings : ScriptableObject
    {
        [Tooltip("Check this to load all audio clips at startup. It does not work when addressables enabled.")]
        [field: SerializeField] public bool PreloadAllSounds { get; set; } = true;
    }
}