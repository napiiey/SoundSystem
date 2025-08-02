using UnityEngine;

namespace Acfeel.SoundSystem
{
    [CreateAssetMenu(fileName = "SoundSystemSettings", menuName = "SoundSystem/Create Settings")]
    public class SoundSystemSettings : ScriptableObject
    {
        [Tooltip("Check this to load all audio clips at startup. It does not work when addressables enabled.")]
        [field: SerializeField] public bool PreloadAllSounds { get; set; } = true;
        [field: SerializeField] public string LootFolderNameInResources { get; set; } = "Audio";
        [field: SerializeField] public string AmbFolderNameInResources { get; set; } = "Amb";
        [field: SerializeField] public string BgmFolderNameInResources { get; set; } = "Bgm";
        [field: SerializeField] public string SeFolderNameInResources { get; set; } = "Se";
        [field: SerializeField] public string SysFolderNameInResources { get; set; } = "Sys";
        
        public string GetFolderNameInResources(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.Amb:
                    return AmbFolderNameInResources;
                case SoundType.Bgm:
                    return BgmFolderNameInResources;
                case SoundType.Se:
                    return SeFolderNameInResources;
                case SoundType.Sys:
                    return SysFolderNameInResources;
                default:
                    return null;
            }
        }
    }
}