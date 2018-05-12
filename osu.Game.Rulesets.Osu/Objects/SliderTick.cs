// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderTick : OsuHitObject
    {
        public int SpanIndex { get; set; }
        public double SpanStartTime { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            double offset;

            if (SpanIndex > 0)
                // Adding 200 to include the offset stable used.
                // This is so on repeats ticks don't appear too late to be visually processed by the player.
                offset = 200;
            else
                offset = TimeFadein * 0.66f;

            TimePreempt = (StartTime - SpanStartTime) / 2 + offset;
        }
    }
}
