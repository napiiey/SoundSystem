using System.Collections.Generic;
using UnityEngine;

namespace Acfeel.SoundSystem
{
    public enum SoundType
    {
        Master,
        BusBgm,
        BusSe,
        Bgm,
        Amb,
        Se,
        Sys
    }

    public enum ChannelType
    {
        Long,
        Short
    }

    public class SoundSystemSettings : ScriptableObject
    {
        [Tooltip("Check this to load all audio clips at startup. It does not work when addressables enabled.")]
        [field: SerializeField]
        public bool PreloadAllSounds { get; set; } = true;

        [Tooltip("Maximum simultaneous playback count for BGM and Ambience")]
        [field: SerializeField] public int LongChannelCount { get; private set; } = 4;
        [Tooltip("Maximum simultaneous playback count for SE and Sys")]
        [field: SerializeField] public int ShortChannelCount { get; private set; } = 16;
        [field: SerializeField] public float DefaultSourceVol { get; private set; } = 0.8f;

        // VolBalance
        [Header("Global volume balance")]
        [SerializeField] float volBalanceMaster = 1.0f;
        [SerializeField] float volBalanceBusBgm = 1.0f;
        [SerializeField] float volBalanceBusSe = 1.0f;
        [SerializeField] float volBalanceBgm = 0.4f;
        [SerializeField] float volBalanceAmb = 1.0f;
        [SerializeField] float volBalanceSe = 1.0f;
        [SerializeField] float volBalanceSys = 1.0f;

        // UserVol
        [Header("Initial values for user volume settings")]
        [SerializeField] float userVolMaster = 1.0f;
        [SerializeField] float userVolBusBgm = 1.0f;
        [SerializeField] float userVolBusSe = 1.0f;
        [SerializeField] float userVolBgm = 0.7f;
        [SerializeField] float userVolAmb = 0.7f;
        [SerializeField] float userVolSe = 0.7f;
        [SerializeField] float userVolSys = 0.7f;

        // DefaultLoop
        [Header("Initial loop settings")]
        [SerializeField] bool defaultLoopBgm = true;
        [SerializeField] bool defaultLoopAmb = true;
        [SerializeField] bool defaultLoopSe;
        [SerializeField] bool defaultLoopSys;

        public Dictionary<SoundType, float> UserVol { get; private set; }
        public Dictionary<SoundType, float> VolBalance { get; private set; }
        public Dictionary<SoundType, bool> DefaultLoop { get; private set; }

        public Dictionary<SoundType, ChannelType> SoundTypeToChannelType { get; } = new()
        {
            { SoundType.Bgm, ChannelType.Long },
            { SoundType.Amb, ChannelType.Long },
            { SoundType.Se, ChannelType.Short },
            { SoundType.Sys, ChannelType.Short }
        };


        public void InitializeDictionaries()
        {
            UserVol = new Dictionary<SoundType, float>
            {
                { SoundType.Master, userVolMaster },
                { SoundType.BusBgm, userVolBusBgm },
                { SoundType.BusSe, userVolBusSe },
                { SoundType.Bgm, userVolBgm },
                { SoundType.Amb, userVolAmb },
                { SoundType.Se, userVolSe },
                { SoundType.Sys, userVolSys }
            };

            VolBalance = new Dictionary<SoundType, float>
            {
                { SoundType.Master, volBalanceMaster },
                { SoundType.BusBgm, volBalanceBusBgm },
                { SoundType.BusSe, volBalanceBusSe },
                { SoundType.Bgm, volBalanceBgm },
                { SoundType.Amb, volBalanceAmb },
                { SoundType.Se, volBalanceSe },
                { SoundType.Sys, volBalanceSys }
            };

            DefaultLoop = new Dictionary<SoundType, bool>
            {
                { SoundType.Bgm, defaultLoopBgm },
                { SoundType.Amb, defaultLoopAmb },
                { SoundType.Se, defaultLoopSe },
                { SoundType.Sys, defaultLoopSys }
            };
        }
    }
}