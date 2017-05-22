// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which has a single hit press.
    /// </summary>
    public class Note : ManiaHitObject
    {
        /// <summary>
        /// The key-press hit window for this note.
        /// </summary>
        protected HitWindows HitWindows = new HitWindows();

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            HitWindows = new HitWindows(difficulty.OverallDifficulty);
        }
    }
}
