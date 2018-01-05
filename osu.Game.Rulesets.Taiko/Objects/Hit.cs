// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Hit : TaikoHitObject
    {
        /// <summary>
        /// The hit window that results in a "GREAT" hit.
        /// </summary>
        public double HitWindowGreat = 35;

        /// <summary>
        /// The hit window that results in a "GOOD" hit.
        /// </summary>
        public double HitWindowGood = 80;

        /// <summary>
        /// The hit window that results in a "MISS".
        /// </summary>
        public double HitWindowMiss = 95;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            HitWindowGreat = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 50, 35, 20);
            HitWindowGood = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 120, 80, 50);
            HitWindowMiss = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 135, 95, 70);
        }
    }
}
