//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects
{
    public abstract class OsuHitObject : HitObject
    {
        public Vector2 Position { get; set; }

        public virtual Vector2 EndPosition => Position;

        [Flags]
        internal enum HitObjectType
        {
            Circle = 1,
            Slider = 2,
            NewCombo = 4,
            CircleNewCombo = 5,
            SliderNewCombo = 6,
            Spinner = 8,
            ColourHax = 122,
            Hold = 128,
            ManiaLong = 128,
        }
    }
}
