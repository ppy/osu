// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class RepeatPoint : OsuHitObject
    {
        public int RepeatIndex { get; set; }
        public double RepeatDuration { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            // We want to show the first RepeatPoint as the TimePreempt dictates but on short (and possibly fast) sliders
            // we may need to cut down this time on following RepeatPoints to only show up to two RepeatPoints at any given time.
            if (RepeatIndex > 1 && TimePreempt > RepeatDuration * 2)
                TimePreempt = (float)RepeatDuration * 2;
        }
    }
}
