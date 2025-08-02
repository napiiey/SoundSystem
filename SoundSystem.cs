using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoundSystem
{
    public class SoundSystem : SingletonMonoBehaviour<SoundSystem>
    {
        readonly Queue<int>[] history = { new(), new() };
        SoundLoader soundLoader;
        AudioSource[] audioSources;
        SoundSystemControl[] controls;
        int channelCount;
        bool mute;
        long frameCounter;
        int idCounter;
        CancellationTokenSource cts;
        public SoundData SoundData { get; set; } = new();
        public SoundSystemSettings Settings { get; private set; }
        public SoundSystemControl Bgm { get; set; }
        public SoundSystemControl Amb { get; set; }

        protected override void Awake()
        {
            base.Awake();
            Settings = Resources.Load<SoundSystemSettings>("SoundSystemSettings");
            if (Settings == null)
            {
                Debug.LogError("SoundSystemSettings.asset not found in Resources folder. Please create one.");
                // Create a default settings object to prevent null reference errors
                Settings = ScriptableObject.CreateInstance<SoundSystemSettings>();
            }

            soundLoader = new SoundLoader(Settings);
            channelCount = SoundData.LongChannelCount + SoundData.ShortChannelCount;
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

        void OnApplicationQuit()
        {
            cts.Cancel();
        }

        async UniTask MainUpdate(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    frameCounter++;

                    if (Bgm != null && Bgm.FileName != "" && Bgm.IsIntroLoop && Bgm.Source.time >= Bgm.LoopEndSec)
                    {
                        Bgm.Source.time = Bgm.LoopStartSec;
                    }

                    await UniTask.DelayFrame(1, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        public SoundSystemControl Play(SoundType soundType, string fileName, float? sourceVol = null)
        {
            int startCh = SoundData.SoundTypeToChannelType[soundType] == ChannelType.Long ? 0 : SoundData.LongChannelCount;
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
                    history[(int)SoundData.SoundTypeToChannelType[soundType]].Enqueue(resultCh);
                    noFreeChanel = false;
                    break;
                }
            }

            if (noFreeChanel)
            {
                resultCh = history[(int)SoundData.SoundTypeToChannelType[soundType]].Dequeue();
                history[(int)SoundData.SoundTypeToChannelType[soundType]].Enqueue(resultCh);
            }

            if (audioSources[resultCh].isPlaying == controls[resultCh].IsPrePlaying)
            {
                controls[resultCh].Stop();
            }

            controls[resultCh].Reset(fileName, soundType, frameCounter, idCounter++);
            controls[resultCh].SourceVol = sourceVol ?? SoundData.DefaultSourceVol;
            audioSources[resultCh].loop = SoundData.DefaultLoop[soundType];
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
                : SoundData.UserVol[SoundType.Master] * SoundData.UserVol[busType] * SoundData.UserVol[soundType]
                  * SoundData.VolBalance[SoundType.Master] * SoundData.VolBalance[busType] *
                  SoundData.VolBalance[soundType];
        }

        public static void SetGlobalVolume(SoundType soundType, float vol)
        {
            Instance.SoundData.UserVol[soundType] = vol;

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
            Instance.SoundData.VolBalance[soundType] = vol;

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
            return SoundData.SoundTypeToChannelType[soundType] == ChannelType.Long
                ? SoundData.LongChannelCount
                : SoundData.ShortChannelCount;
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
