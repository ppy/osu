// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko.Objects
{
    public class Bash : TaikoHitObject
    {
        public override double EndTime => StartTime + Length;

        /// <summary>
        /// Old osu! constant that increases number of "spins" required.
        /// </summary>
        private const double spinner_ratio = 1.65;

        /// <summary>
        /// The length of the bash in time units.
        /// </summary>
        public double Length;

        /// <summary>
        /// The number of hits required to complete the bash successfully.
        /// </summary>
        public int RequiredHits;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            double spinnerRotationRatio = Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 3, 5, 7.5, Mods.None);
            RequiredHits = (int)Math.Max(1, Length / 1000f * spinnerRotationRatio * spinner_ratio);
        }

        public override TaikoHitType Type => TaikoHitType.Bash;
    }
}
