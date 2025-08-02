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
        int allChannels;
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
            allChannels = SoundData.LongChannelCount + SoundData.ShortChannelCount;
            audioSources = new AudioSource[allChannels];
            controls = new SoundSystemControl[allChannels];

            for (int i = 0; i < allChannels; i++)
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

        public void StopAll(SoundType? soundType = null)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (soundType != null && controls[i].SoundType != soundType) continue;
                controls[i].Stop();
            }
        }

        public void FadeOutAll(float sec, SoundType? soundType = null)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                if (soundType != null && controls[i].SoundType != soundType) continue;
                controls[i].FadeOut(sec);
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

        public void SetGlobalVolume(SoundType soundType, float vol)
        {
            SoundData.UserVol[soundType] = vol;

            for (int i = 0; i < allChannels; i++)
            {
                if (controls[i].SoundType == soundType)
                {
                    controls[i].SetVolume();
                }
            }
        }

        public void SetInnerVolume(SoundType soundType, float vol)
        {
            SoundData.VolBalance[soundType] = vol;

            for (int i = 0; i < allChannels; i++)
            {
                if (controls[i].SoundType == soundType)
                {
                    controls[i].SetVolume();
                }
            }
        }

        public void SetMute(bool boolean)
        {
            mute = boolean;

            for (int i = 0; i < allChannels; i++)
            {
                controls[i].SetVolume();
            }
        }

        public int GetChCapacity(SoundType soundType)
        {
            return SoundData.SoundTypeToChannelType[soundType] == ChannelType.Long
                ? SoundData.LongChannelCount
                : SoundData.ShortChannelCount;
        }

        public SoundSystemControl PlayBgm(string fileName, float? volume = null)
        {
            return Play(SoundType.Bgm, fileName, volume);
        }

        public SoundSystemControl PlayAmb(string fileName, float? volume = null)
        {
            return Play(SoundType.Amb, fileName, volume);
        }

        public SoundSystemControl PlaySe(string fileName, float? volume = null)
        {
            return Play(SoundType.Se, fileName, volume);
        }

        public SoundSystemControl PlaySys(string fileName, float? volume = null)
        {
            return Play(SoundType.Sys, fileName, volume);
        }

        public void PlayMainBgm(string fileName, float? volume = null)
        {
            Bgm = Play(SoundType.Bgm, fileName, volume);
        }

        public void PlayMainAmb(string fileName, float? volume = null)
        {
            Amb = Play(SoundType.Amb, fileName, volume);
        }
    }
}
