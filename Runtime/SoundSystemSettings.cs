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
        [SerializeField] private float volBalanceMaster = 1.0f;
        [SerializeField] private float volBalanceBusBgm = 1.0f;
        [SerializeField] private float volBalanceBusSe = 1.0f;
        [SerializeField] private float volBalanceBgm = 0.4f;
        [SerializeField] private float volBalanceAmb = 1.0f;
        [SerializeField] private float volBalanceSe = 1.0f;
        [SerializeField] private float volBalanceSys = 1.0f;
        
        // UserVol
        [Header("Initial values for user volume settings")]
        [SerializeField] private float userVolMaster = 1.0f;
        [SerializeField] private float userVolBusBgm = 1.0f;
        [SerializeField] private float userVolBusSe = 1.0f;
        [SerializeField] private float userVolBgm = 0.7f;
        [SerializeField] private float userVolAmb = 0.7f;
        [SerializeField] private float userVolSe = 0.7f;
        [SerializeField] private float userVolSys = 0.7f;

        // DefaultLoop
        [Header("Initial loop settings")]
        [SerializeField] private bool defaultLoopBgm = true;
        [SerializeField] private bool defaultLoopAmb = true;
        [SerializeField] private bool defaultLoopSe = false;
        [SerializeField] private bool defaultLoopSys = false;

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
