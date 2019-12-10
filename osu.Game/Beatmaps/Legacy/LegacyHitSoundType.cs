using System;

namespace osu.Game.Beatmaps.Legacy
{
    [Flags]
    internal enum LegacyHitSoundType
    {
        None = 0,
        Normal = 1,
        Whistle = 2,
        Finish = 4,
        Clap = 8
    }
}
