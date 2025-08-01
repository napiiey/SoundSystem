using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if SOUNDSYSTEM_ADDRESSABLES_SUPPORT
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Acfeel.SoundSystem
{
    public class SoundLoader
    {
        readonly Dictionary<string, AudioClip> audioClips = new();
        readonly SoundSystemSettings settings;

        public SoundLoader(SoundSystemSettings settings)
        {
            this.settings = settings;
            if (this.settings.PreloadAllSounds)
            {
                PreloadAllClips();
            }
        }

#if SOUNDSYSTEM_ADDRESSABLES_SUPPORT
        public void PreloadAllClips()
        {
            // Addressablesでは事前ロードを別の明示的なメソッドで管理することが多いため全体事前ロードは実装しません。
        }

        public async UniTask<AudioClip> LoadAudioClip(string fileName, SoundType soundType)
        {
            if (audioClips.TryGetValue(fileName, out var clip))
            {
                return clip;
            }

            var handle = Addressables.LoadAssetAsync<AudioClip>(fileName);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                audioClips[fileName] = handle.Result;
                return handle.Result;
            }
            else
            {
                Debug.LogError($"Failed to load AudioClip from Addressables: {fileName}");
                return null;
            }
        }
#else
        public void PreloadAllClips()
        {
            var clips = Resources.LoadAll<AudioClip>("SoundSystem");
            foreach (AudioClip clip in clips)
            {
                audioClips.TryAdd(clip.name, clip);
            }
        }

        public async UniTask<AudioClip> LoadAudioClip(string fileName, SoundType soundType)
        {
            if (audioClips.TryGetValue(fileName, out AudioClip clip))
            {
                return await UniTask.FromResult(clip);
            }

            // 事前ロードしていない場合は、オンデマンドでロード
            if (!settings.PreloadAllSounds)
            {
                var sb = new StringBuilder();
                sb.Append("SoundSystem/");
                sb.Append(GetFolderNameInResources(soundType));
                sb.Append('/');
                sb.Append(fileName);
                string path = sb.ToString();
                Debug.Log(path);
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    audioClips[fileName] = clip; // キャッシュに保存
                    return await UniTask.FromResult(clip);
                }
            }

            Debug.LogError($"Failed to load AudioClip from Resources: {fileName}");
            return await UniTask.FromResult<AudioClip>(null);
        }

        string GetFolderNameInResources(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.Amb:
                    return "Amb";
                case SoundType.Bgm:
                    return "Bgm";
                case SoundType.Se:
                    return "Se";
                case SoundType.Sys:
                    return "Sys";
                default:
                    return null;
            }
        }
#endif
    }
}