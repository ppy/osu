// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderTick : OsuHitObject
    {
        public int SpanIndex { get; set; }
        public double SliderStartTime { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            // SliderTicks appear earlier and earlier going further into a Slider.
            TimePreempt = StartTime - ((StartTime - SliderStartTime) / 2 + SliderStartTime - TimeFadein * 0.66f);
        }
    }
}
