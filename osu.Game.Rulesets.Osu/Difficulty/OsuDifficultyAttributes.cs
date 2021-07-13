// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double[] AimComboBasedDifficulties { get; set; }
        public double[] AimMissCounts { get; set; }

        public double[] SpeedComboBasedDifficulties { get; set; }
        public double[] SpeedMissCounts { get; set; }

        public double ApproachRate { get; set; }
        public double OverallDifficulty { get; set; }
        public int HitCircleCount { get; set; }
        public int HitSliderCount { get; set; }
        public int SpinnerCount { get; set; }
    }
}
