using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Acfeel.SoundSystem
{
    public class SoundSystemControl
    {
        readonly SoundSystem soundSystem;
        bool isShuttingDown;

        public SoundSystemControl(SoundSystem system, AudioSource source, int channel)
        {
            soundSystem = system;
            Source = source;
            ch = channel;
            SourceVol = soundSystem.Settings.DefaultSourceVol;
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
            isShuttingDown = false;
            CancelAllTokenSources();
            FileName = file;
            SoundType = soundType;
            SourceVol = soundSystem.Settings.DefaultSourceVol;
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
            var cts = ReplaceCancellationTokenSource("fade");
            if (cts == null) return this;
            FadeProcess(1.0f, false, cts.Token, fadeSec).Forget();
            return this;
        }

        public SoundSystemControl FadeOut(float fadeSec = 1.0f)
        {
            var cts = ReplaceCancellationTokenSource("fade");
            if (cts == null) return this;
            FadeProcess(0.0f, true, cts.Token, fadeSec).Forget();
            return this;
        }

        public SoundSystemControl Fade(float endVol, float fadeSec = 1.0f)
        {
            var cts = ReplaceCancellationTokenSource("fade");
            if (cts == null) return this;
            FadeProcess(endVol, false, cts.Token, fadeSec).Forget();
            return this;
        }

        async UniTask FadeProcess(float endVol, bool stop, CancellationToken token, float fadeSec)
        {
            try
            {
                if (!CanAccessSource()) return;

                float startVol = FaderVol;
                float time = 0.0f;

                while (time < fadeSec)
                {
                    if (token.IsCancellationRequested || !CanAccessSource()) return;

                    time += Time.deltaTime;
                    FaderVol = startVol + (endVol - startVol) * EaseInOutQuad(time / fadeSec);
                    SetVolume();
                    await UniTask.DelayFrame(1, cancellationToken: token);
                }

                if (token.IsCancellationRequested || !CanAccessSource()) return;

                FaderVol = endVol;
                SetVolume();
                if (stop) Stop();
            }
            catch (OperationCanceledException)
            {
            }
        }

        float EaseInOutQuad(float t)
        {
            if (t <= 0.5f) return 2.0f * t * t;
            t -= 0.5f;
            return 2.0f * t * (1.0f - t) + 0.5f;
        }

        public void SetVolume()
        {
            if (!CanAccessSource()) return;
            Source.volume = SourceVol * FaderVol * soundSystem.GetGlobalVolume(SoundType);
        }

        public void Stop()
        {
            CancelAllTokenSources();
            if (IsIntroLoop)
            {
                IsIntroLoop = false;
            }

            if (IsPrePlaying)
            {
                IsPrePlaying = false;
            }

            if (Source != null)
            {
                Source.Stop();
            }
        }

        public SoundSystemControl Delay(float delaySec)
        {
            var cts = ReplaceCancellationTokenSource("delay");
            if (cts == null) return this;
            if (!CanAccessSource()) return this;

            Source.mute = true;
            IsPrePlaying = true;
            DelayPlayStart(cts.Token, delaySec).Forget();
            return this;
        }

        async UniTask DelayPlayStart(CancellationToken token, float delaySec)
        {
            try
            {
                int delayTime = (int)(delaySec * 1000);
                await UniTask.Delay(delayTime, cancellationToken: token);
                if (token.IsCancellationRequested || !CanAccessSource()) return;

                Source.Stop();
                Source.mute = false;
                Source.Play();
                IsPrePlaying = false;
            }
            catch (OperationCanceledException)
            {
            }
        }

        public SoundSystemControl SetPitch(float pitch = 1f)
        {
            if (!CanAccessSource()) return this;
            Source.pitch = pitch;
            return this;
        }

        public SoundSystemControl SetPan(float pan = 0f)
        {
            if (!CanAccessSource()) return this;
            Source.panStereo = pan;
            return this;
        }

        public SoundSystemControl SetSpatial(float spatial = 0f)
        {
            if (!CanAccessSource()) return this;
            Source.spatialBlend = spatial;
            return this;
        }

        public SoundSystemControl SetLoop(bool boolean = true)
        {
            if (!CanAccessSource()) return this;
            Source.loop = boolean;
            return this;
        }

        public SoundSystemControl SetIntroLoop(float loopStartSec, float loopEndSec)
        {
            IsIntroLoop = true;
            LoopStartSec = loopStartSec;
            LoopEndSec = loopEndSec;
            return this;
        }

        public SoundSystemControl SetIntroLoopBpm(float bpm, float loopStartSec, float count)
        {
            IsIntroLoop = true;
            Bpm = bpm;
            LoopStartSec = loopStartSec;
            LoopEndSec = loopStartSec + 60f / bpm * count;
            return this;
        }

        public float GetTimeOnBeats()
        {
            return CanAccessSource() ? Source.time * Bpm / 60 : 0.0f;
        }

        internal void Shutdown()
        {
            isShuttingDown = true;
            CancelAllTokenSources();
        }

        internal bool HasValidSource()
        {
            return CanAccessSource();
        }

        CancellationTokenSource ReplaceCancellationTokenSource(string key)
        {
            if (isShuttingDown) return null;

            CancelAndDisposeTokenSource(key);

            var cts = new CancellationTokenSource();
            Cts[key] = cts;
            return cts;
        }

        void CancelAllTokenSources()
        {
            if (Cts.Count == 0) return;

            foreach (var key in new List<string>(Cts.Keys))
            {
                CancelAndDisposeTokenSource(key);
            }
        }

        void CancelAndDisposeTokenSource(string key)
        {
            if (!Cts.TryGetValue(key, out var cts)) return;

            Cts.Remove(key);
            cts.Cancel();
            cts.Dispose();
        }

        bool CanAccessSource()
        {
            return !isShuttingDown && Source != null;
        }
    }
}
