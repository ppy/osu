using System;

namespace osu.Game.Modes.Objects.Types
{
    [Flags]
    public enum HitObjectType
    {
        Circle = 1 << 0,
        Slider = 1 << 1,
        NewCombo = 1 << 2,
        Spinner = 1 << 3,
        ColourHax = 122,
        Hold = 1 << 7,
        SliderTick = 1 << 8,
    }
}