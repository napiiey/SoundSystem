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
        public SoundSystemSettings Settings { get; private set; }
        public SoundSystemControl Bgm { get; set; }
        public static SoundSystemControl MainBgm => Instance.Bgm;
        public SoundSystemControl Amb { get; set; }
        public static SoundSystemControl MainAmb => Instance.Amb;

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

        void OnDestroy()
        {
            ProcessDestroy();
        }

        void OnApplicationQuit()
        {
            ProcessDestroy();
        }

        void ProcessDestroy()
        {
            cts?.Cancel();
            foreach (SoundSystemControl control in controls)
            {
                control.Stop();
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
                        MainBgm.Source.time >= MainBgm.LoopEndSec)
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

            for (int ch = startCh; ch <= lastCh; ch++)
            {
                if (controls[ch].FileName == fileName && controls[ch].StartFrame == frameCounter)
                {
                    Debug.Log("SoundSystem 同一フレーム同ファイル再生");
                    return controls[ch];
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

            if (clip != null)
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
            _ = Instance.soundLoader.LoadAudioClip(fileName, SoundType.Se);
        }

        public static void LoadAllForResources()
        {
            Instance.soundLoader.PreloadAllClips();
        }

        public static void StopAll(SoundType? soundType = null)
        {
            for (int i = 0; i < Instance.audioSources.Length; i++)
            {
                if (soundType != null && Instance.controls[i].SoundType != soundType) continue;
                Instance.controls[i].Stop();
            }
        }

        public static void FadeOutAll(float sec, SoundType? soundType = null)
        {
            for (int i = 0; i < Instance.controls.Length; i++)
            {
                if (soundType != null && Instance.controls[i].SoundType != soundType) continue;
                Instance.controls[i].FadeOut(sec);
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
            Instance.Settings.UserVol[soundType] = vol;

            for (int i = 0; i < Instance.channelCount; i++)
            {
                if (Instance.controls[i].SoundType == soundType)
                {
                    Instance.controls[i].SetVolume();
                }
            }
        }

        public static void SetInnerVolume(SoundType soundType, float vol)
        {
            Instance.Settings.VolBalance[soundType] = vol;

            for (int i = 0; i < Instance.channelCount; i++)
            {
                if (Instance.controls[i].SoundType == soundType)
                {
                    Instance.controls[i].SetVolume();
                }
            }
        }

        public static void SetMute(bool boolean)
        {
            Instance.mute = boolean;

            for (int i = 0; i < Instance.channelCount; i++)
            {
                Instance.controls[i].SetVolume();
            }
        }

        public int GetChCapacity(SoundType soundType)
        {
            return Settings.SoundTypeToChannelType[soundType] == ChannelType.Long
                ? Settings.LongChannelCount
                : Settings.ShortChannelCount;
        }

        public static SoundSystemControl PlayBgm(string fileName, float? volume = null)
        {
            return Instance.Play(SoundType.Bgm, fileName, volume);
        }

        public static SoundSystemControl PlayAmb(string fileName, float? volume = null)
        {
            return Instance.Play(SoundType.Amb, fileName, volume);
        }

        public static SoundSystemControl PlaySe(string fileName, float? volume = null)
        {
            return Instance.Play(SoundType.Se, fileName, volume);
        }

        public static SoundSystemControl PlaySys(string fileName, float? volume = null)
        {
            return Instance.Play(SoundType.Sys, fileName, volume);
        }

        public static void PlayMainBgm(string fileName, float? volume = null)
        {
            Instance.Bgm = Instance.Play(SoundType.Bgm, fileName, volume);
        }

        public static void PlayMainAmb(string fileName, float? volume = null)
        {
            Instance.Amb = Instance.Play(SoundType.Amb, fileName, volume);
        }

        public static void LoadBgm(string fileName)
        {
            Instance.Load(fileName, SoundType.Bgm);
        }

        public static void LoadAmb(string fileName)
        {
            Instance.Load(fileName, SoundType.Amb);
        }

        public static void LoadSe(string fileName)
        {
            Instance.Load(fileName, SoundType.Se);
        }

        public static void LoadSys(string fileName)
        {
            Instance.Load(fileName, SoundType.Sys);
        }
    }
}