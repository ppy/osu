﻿namespace osu.Game.Modes.Osu.Objects
{
    public class SliderTick : OsuHitObject
    {
        public int RepeatIndex { get; set; }

        public override HitObjectType Type => HitObjectType.SliderTick;
    }
}
