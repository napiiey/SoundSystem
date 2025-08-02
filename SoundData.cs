using System.Collections.Generic;

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

    public class SoundData
    {
        public const int LongChannelCount = 4;
        public const int ShortChannelCount = 16;
        public const float DefaultSourceVol = 0.8f;

        public Dictionary<SoundType, float> UserVol { get; set; } = new()
        {
            { SoundType.Master, 1.0f },
            { SoundType.BusBgm, 1.0f },
            { SoundType.BusSe, 1.0f },
            { SoundType.Bgm, 0.7f },
            { SoundType.Amb, 0.7f },
            { SoundType.Se, 0.7f },
            { SoundType.Sys, 0.7f }
        };

        public Dictionary<SoundType, float> VolBalance { get; set; } = new()
        {
            { SoundType.Master, 1.0f },
            { SoundType.BusBgm, 1.0f },
            { SoundType.BusSe, 1.0f },
            { SoundType.Bgm, 0.4f },
            { SoundType.Amb, 1.0f },
            { SoundType.Se, 1.0f },
            { SoundType.Sys, 1.0f }
        };

        public Dictionary<SoundType, ChannelType> SoundTypeToChannelType { get; set; } = new()
        {
            { SoundType.Bgm, ChannelType.Long },
            { SoundType.Amb, ChannelType.Long },
            { SoundType.Se, ChannelType.Short },
            { SoundType.Sys, ChannelType.Short }
        };

        public Dictionary<SoundType, bool> DefaultLoop { get; set; } = new()
        {
            { SoundType.Bgm, true },
            { SoundType.Amb, true },
            { SoundType.Se, false },
            { SoundType.Sys, false }
        };
    }
}
