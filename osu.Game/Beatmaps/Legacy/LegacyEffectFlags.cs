using System;

namespace osu.Game.Beatmaps.Legacy
{
    [Flags]
    internal enum LegacyEffectFlags
    {
        None = 0,
        Kiai = 1,
        OmitFirstBarLine = 8
    }
}
