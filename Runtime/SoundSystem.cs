using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Acfeel.SoundSystem
{
    public class SoundSystem : SingletonMonoBehaviour<SoundSystem>
    {
        readonly Queue<int>[] history = new Queue<int>[Enum.GetValues(typeof(ChannelType)).Length];
        SoundLoader soundLoader;
        AudioSource[] audioSources;
        SoundSystemControl[] controls;
        int channelCount;
        bool mute;
        long frameCounter;
        int idCounter;
        CancellationTokenSource cts;
        bool isDestroying;
        public SoundSystemSettings Settings { get; private set; }
        public SoundSystemControl Bgm { get; set; }
        public static SoundSystemControl MainBgm => Instance ? Instance.Bgm : null;
        public SoundSystemControl Amb { get; set; }
        public static SoundSystemControl MainAmb => Instance ? Instance.Amb : null;

        protected override void Awake()
        {
            base.Awake();
            Settings = Resources.Load<SoundSystemSettings>("SoundSystem/SoundSystemSettings");
            if (Settings == null)
            {
                Debug.LogError("SoundSystemSettings.asset not found in Resources folder. Please create one.");
                // Create a default settings object to prevent null reference errors
                Settings = ScriptableObject.CreateInstance<SoundSystemSettings>();
            }

            Settings.InitializeDictionaries();

            for (int i = 0; i < history.Length; i++)
            {
                history[i] = new Queue<int>();
            }

            soundLoader = new SoundLoader(Settings);
            channelCount = Settings.LongChannelCount + Settings.ShortChannelCount;
            audioSources = new AudioSource[channelCount];
            controls = new SoundSystemControl[channelCount];

            for (int i = 0; i < channelCount; i++)
            {
                audioSources[i] = gameObject.AddComponent<AudioSource>();
                audioSources[i].playOnAwake = false;
                controls[i] = new SoundSystemControl(this, audioSources[i], i);
            }
        }

        void Start()
        {
            cts = new CancellationTokenSource();
            MainUpdate(cts.Token).Forget();
        }

        protected override void OnDestroy()
        {
            ProcessDestroy();
            base.OnDestroy();
        }

        void OnApplicationQuit()
        {
            ProcessDestroy();
        }

        void ProcessDestroy()
        {
            if (isDestroying) return;
            isDestroying = true;

            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            if (controls == null) return;

            foreach (SoundSystemControl control in controls)
            {
                control?.Shutdown();
                control?.Stop();
            }
        }

        async UniTask MainUpdate(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    frameCounter++;

                    if (MainBgm != null && MainBgm.FileName != "" && MainBgm.IsIntroLoop &&
                        MainBgm.HasValidSource() && MainBgm.Source.time >= MainBgm.LoopEndSec)
                    {
                        MainBgm.Source.time = MainBgm.LoopStartSec;
                    }

                    await UniTask.DelayFrame(1, cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public SoundSystemControl Play(SoundType soundType, string fileName, float? sourceVol = null)
        {
            int startCh = Settings.SoundTypeToChannelType[soundType] == ChannelType.Long
                ? 0
                : Settings.LongChannelCount;
            int lastCh = startCh + GetChCapacity(soundType) - 1;
            int resultCh = 0;
            bool noFreeChanel = true;
            bool shouldBlockDuplicateInSameFrame = soundType == SoundType.Se || soundType == SoundType.Sys;

            if (shouldBlockDuplicateInSameFrame)
            {
                for (int ch = startCh; ch <= lastCh; ch++)
                {
                    if (controls[ch].FileName == fileName && controls[ch].StartFrame == frameCounter)
                    {
                        // SE / SYS は同一フレームの同一ファイル多重再生を防ぐ
                        return controls[ch];
                    }
                }
            }

            for (int ch = startCh; ch <= lastCh; ch++)
            {
                if (!audioSources[ch].isPlaying && !controls[ch].IsPrePlaying)
                {
                    resultCh = ch;
                    history[(int)Settings.SoundTypeToChannelType[soundType]].Enqueue(resultCh);
                    noFreeChanel = false;
                    break;
                }
            }

            if (noFreeChanel)
            {
                resultCh = history[(int)Settings.SoundTypeToChannelType[soundType]].Dequeue();
                history[(int)Settings.SoundTypeToChannelType[soundType]].Enqueue(resultCh);
            }

            if (audioSources[resultCh].isPlaying == controls[resultCh].IsPrePlaying)
            {
                controls[resultCh].Stop();
            }

            controls[resultCh].Reset(fileName, soundType, frameCounter, idCounter++);
            controls[resultCh].SourceVol = sourceVol ?? Settings.DefaultSourceVol;
            audioSources[resultCh].loop = Settings.DefaultLoop[soundType];
            audioSources[resultCh].panStereo = 0.0f;
            audioSources[resultCh].pitch = 1.0f;
            controls[resultCh].SetVolume();

            LoadAndPlay(resultCh, fileName, soundType).Forget();

            return controls[resultCh];
        }

        async UniTaskVoid LoadAndPlay(int channel, string fileName, SoundType soundType)
        {
            AudioClip clip = await soundLoader.LoadAudioClip(fileName, soundType);

            if (clip != null && !isDestroying && controls != null &&
                channel >= 0 && channel < controls.Length &&
                controls[channel] != null && controls[channel].HasValidSource())
            {
                audioSources[channel].clip = clip;
                audioSources[channel].Play();
            }
        }

        public void Load(string fileName, SoundType soundType)
        {
            _ = soundLoader.LoadAudioClip(fileName, soundType);
        }

        public static void LoadForAddressables(string fileName)
        {
            var instance = Instance;
            if (!instance) return;
            _ = instance.soundLoader.LoadAudioClip(fileName, SoundType.Se);
        }

        public static void LoadAllForResources()
        {
            var instance = Instance;
            if (!instance) return;
            instance.soundLoader.PreloadAllClips();
        }

        public static void StopAll(SoundType? soundType = null)
        {
            var instance = Instance;
            if (!instance) return;

            for (int i = 0; i < instance.audioSources.Length; i++)
            {
                if (soundType != null && instance.controls[i].SoundType != soundType) continue;
                instance.controls[i].Stop();
            }
        }

        public static void FadeOutAll(float sec, SoundType? soundType = null)
        {
            var instance = Instance;
            if (!instance) return;

            for (int i = 0; i < instance.controls.Length; i++)
            {
                if (soundType != null && instance.controls[i].SoundType != soundType) continue;
                instance.controls[i].FadeOut(sec);
            }
        }

        public float GetGlobalVolume(SoundType soundType)
        {
            SoundType busType = soundType == SoundType.Bgm || soundType == SoundType.Amb
                ? SoundType.BusBgm
                : SoundType.BusSe;
            return mute
                ? 0.0f
                : Settings.UserVol[SoundType.Master] * Settings.UserVol[busType] * Settings.UserVol[soundType]
                  * Settings.VolBalance[SoundType.Master] * Settings.VolBalance[busType] *
                  Settings.VolBalance[soundType];
        }

        public static void SetGlobalVolume(SoundType soundType, float vol)
        {
            var instance = Instance;
            if (!instance) return;

            instance.Settings.UserVol[soundType] = vol;

            for (int i = 0; i < instance.channelCount; i++)
            {
                if (instance.IsAffectedByVolumeChange(soundType, instance.controls[i].SoundType))
                {
                    instance.controls[i].SetVolume();
                }
            }
        }

        public static void SetInnerVolume(SoundType soundType, float vol)
        {
            var instance = Instance;
            if (!instance) return;

            instance.Settings.VolBalance[soundType] = vol;

            for (int i = 0; i < instance.channelCount; i++)
            {
                if (instance.IsAffectedByVolumeChange(soundType, instance.controls[i].SoundType))
                {
                    instance.controls[i].SetVolume();
                }
            }
        }

        public static void SetMute(bool boolean)
        {
            var instance = Instance;
            if (!instance) return;

            instance.mute = boolean;

            for (int i = 0; i < instance.channelCount; i++)
            {
                instance.controls[i].SetVolume();
            }
        }

        public int GetChCapacity(SoundType soundType)
        {
            return Settings.SoundTypeToChannelType[soundType] == ChannelType.Long
                ? Settings.LongChannelCount
                : Settings.ShortChannelCount;
        }

        bool IsAffectedByVolumeChange(SoundType changedType, SoundType targetType)
        {
            return changedType switch
            {
                SoundType.Master => true,
                SoundType.BusBgm => targetType == SoundType.Bgm || targetType == SoundType.Amb,
                SoundType.BusSe => targetType == SoundType.Se || targetType == SoundType.Sys,
                _ => targetType == changedType
            };
        }

        public static SoundSystemControl PlayBgm(string fileName, float? volume = null)
        {
            var instance = Instance;
            return instance ? instance.Play(SoundType.Bgm, fileName, volume) : null;
        }

        public static SoundSystemControl PlayAmb(string fileName, float? volume = null)
        {
            var instance = Instance;
            return instance ? instance.Play(SoundType.Amb, fileName, volume) : null;
        }

        public static SoundSystemControl PlaySe(string fileName, float? volume = null)
        {
            var instance = Instance;
            return instance ? instance.Play(SoundType.Se, fileName, volume) : null;
        }

        public static SoundSystemControl PlaySys(string fileName, float? volume = null)
        {
            var instance = Instance;
            return instance ? instance.Play(SoundType.Sys, fileName, volume) : null;
        }

        public static void PlayMainBgm(string fileName, float? volume = null)
        {
            var instance = Instance;
            if (!instance) return;
            if (instance.Bgm != null && instance.Bgm.FileName == fileName && instance.IsControlActive(instance.Bgm)) return;
            instance.Bgm?.Stop();
            instance.Bgm = instance.Play(SoundType.Bgm, fileName, volume);
        }

        public static void PlayMainAmb(string fileName, float? volume = null)
        {
            var instance = Instance;
            if (!instance) return;
            if (instance.Amb != null && instance.Amb.FileName == fileName && instance.IsControlActive(instance.Amb)) return;
            instance.Amb?.Stop();
            instance.Amb = instance.Play(SoundType.Amb, fileName, volume);
        }

        public static void LoadBgm(string fileName)
        {
            var instance = Instance;
            if (!instance) return;
            instance.Load(fileName, SoundType.Bgm);
        }

        public static void LoadAmb(string fileName)
        {
            var instance = Instance;
            if (!instance) return;
            instance.Load(fileName, SoundType.Amb);
        }

        public static void LoadSe(string fileName)
        {
            var instance = Instance;
            if (!instance) return;
            instance.Load(fileName, SoundType.Se);
        }

        public static void LoadSys(string fileName)
        {
            var instance = Instance;
            if (!instance) return;
            instance.Load(fileName, SoundType.Sys);
        }

        bool IsControlActive(SoundSystemControl control)
        {
            return control != null &&
                control.HasValidSource() &&
                (control.IsPrePlaying || control.Source.isPlaying);
        }
    }
}
