// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Modes.Osu.Objects
{
    public class SliderTick : OsuHitObject
    {
        public int RepeatIndex { get; set; }

        public override HitObjectType Type => HitObjectType.SliderTick;
    }
}
