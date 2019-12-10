using System;

namespace osu.Game.Beatmaps.Legacy
{
    [Flags]
    internal enum LegacyHitObjectType
    {
        Circle = 1,
        Slider = 1 << 1,
        NewCombo = 1 << 2,
        Spinner = 1 << 3,
        ComboOffset = (1 << 4) | (1 << 5) | (1 << 6),
        Hold = 1 << 7
    }
}
