using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Acfeel.SoundSystem
{
    public class SoundSystemControl
    {
        readonly SoundSystem soundSystem;
        int ch;

        public SoundSystemControl(SoundSystem system, AudioSource source, int channel)
        {
            soundSystem = system;
            Source = source;
            ch = channel;
            SourceVol = SoundData.DefaultSourceVol;
        }

        public string FileName { get; set; } = "";
        public SoundType SoundType { get; set; }
        public float SourceVol { get; set; }
        public float FaderVol { get; set; } = 1.0f;
        public float Pitch { get; set; } = 1.0f;
        public float Pan { get; set; }
        public float Spatial { get; set; }
        public long StartFrame { get; set; }
        public int ID { get; set; }
        public Dictionary<string, CancellationTokenSource> Cts { get; set; } = new();
        public float LoopStartSec { get; set; }
        public float LoopEndSec { get; set; }
        public float Bpm { get; set; } = 120f;
        public bool IsIntroLoop { get; set; }
        public bool IsPrePlaying { get; set; }

        public AudioSource Source { get; }

        public void Reset(string file, SoundType soundType, long frameCounter, int idCounter)
        {
            FileName = file;
            SoundType = soundType;
            SourceVol = SoundData.DefaultSourceVol;
            FaderVol = 1.0f;
            Pitch = 1.0f;
            Pan = 0.0f;
            Spatial = 0.0f;
            StartFrame = frameCounter;
            ID = idCounter;
            Cts = new Dictionary<string, CancellationTokenSource>();
            IsPrePlaying = true;
        }

        public SoundSystemControl FadeIn(float fadeSec = 1.0f)
        {
            FaderVol = 0.0f;
            Cts.Remove("fade");
            Cts.Add("fade", new CancellationTokenSource());
            FadeProcess(1.0f, false, Cts["fade"].Token, fadeSec).Forget();
            return this;
        }

        public SoundSystemControl FadeOut(float fadeSec = 1.0f)
        {
            Cts.Remove("fade");
            Cts.Add("fade", new CancellationTokenSource());
            FadeProcess(0.0f, true, Cts["fade"].Token, fadeSec).Forget();
            return this;
        }

        public SoundSystemControl Fade(float endVol, float fadeSec = 1.0f)
        {
            Cts.Remove("fade");
            Cts.Add("fade", new CancellationTokenSource());
            FadeProcess(endVol, false, Cts["fade"].Token, fadeSec).Forget();
            return this;
        }

        async UniTask FadeProcess(float endVol, bool stop, CancellationToken token, float fadeSec)
        {
            float startVol = FaderVol;
            float time = 0.0f;

            while (time < fadeSec)
            {
                time += Time.deltaTime;
                FaderVol = startVol + (endVol - startVol) * EaseInOutQuad(time / fadeSec);
                SetVolume();
                await UniTask.DelayFrame(1, cancellationToken: token);
            }

            FaderVol = endVol;
            if (stop) Stop();
        }

        float EaseInOutQuad(float t)
        {
            if (t <= 0.5f) return 2.0f * t * t;
            t -= 0.5f;
            return 2.0f * t * (1.0f - t) + 0.5f;
        }

        public void SetVolume()
        {
            Source.volume = SourceVol * FaderVol * soundSystem.GetGlobalVolume(SoundType);
        }

        public void Stop()
        {
            if (Cts.ContainsKey("fade"))
            {
                Cts["fade"].Cancel();
            }

            if (Cts.ContainsKey("delay"))
            {
                Cts["delay"].Cancel();
            }

            Source.Stop();
            if (IsIntroLoop)
            {
                IsIntroLoop = false;
            }

            if (IsPrePlaying)
            {
                IsPrePlaying = false;
            }
        }

        public SoundSystemControl Delay(float delaySec)
        {
            Source.mute = true;
            IsPrePlaying = true;
            Cts.Remove("delay");
            Cts.Add("delay", new CancellationTokenSource());
            DelayPlayStart(Cts["delay"].Token, delaySec).Forget();
            return this;
        }

        async UniTask DelayPlayStart(CancellationToken token, float delaySec)
        {
            int delayTime = (int)(delaySec * 1000);
            await UniTask.Delay(delayTime, cancellationToken: token);
            Source.Stop();
            Source.mute = false;
            Source.Play();
            IsPrePlaying = false;
        }

        public SoundSystemControl SetPitch(float pitch = 1f)
        {
            Source.pitch = pitch;
            return this;
        }

        public SoundSystemControl SetPan(float pan = 0f)
        {
            Source.panStereo = pan;
            return this;
        }

        public SoundSystemControl SetSpatial(float spatial = 0f)
        {
            Source.spatialBlend = spatial;
            return this;
        }

        public SoundSystemControl SetLoop(bool boolean = true)
        {
            Source.loop = boolean;
            return this;
        }

        public SoundSystemControl SetIntroLoop(float loopStart, float loopEnd)
        {
            IsIntroLoop = true;
            LoopStartSec = loopStart;
            LoopEndSec = loopEnd;
            return this;
        }

        public SoundSystemControl SetIntroLoopBpm(float bpm, float loopStart, float count)
        {
            IsIntroLoop = true;
            Bpm = bpm;
            LoopStartSec = loopStart;
            LoopEndSec = loopStart + 60f / bpm * count;
            return this;
        }

        public float GetTimeOnBeats()
        {
            return Source.time * Bpm / 60;
        }
    }
}
