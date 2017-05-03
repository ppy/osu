// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class Note : ManiaHitObject
    {
        public HitWindows HitWindows { get; protected set; } = new HitWindows();

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            HitWindows = new HitWindows(difficulty.OverallDifficulty);
        }
    }
}
