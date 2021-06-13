// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double TapStarRating { get; set; }
        public double TapDifficulty { get; set; }
        public double StreamNoteCount { get; set; }
        public double MashTapDifficulty { get; set; }

        public double FingerControlStarRating { get; set; }
        public double FingerControlDifficulty { get; set; }

        public double AimStarRating { get; set; }
        public double AimDifficulty { get; set; }
        public double AimHiddenFactor { get; set; }
        public double[] ComboThroughputs { get; set; }
        public double[] MissThroughputs { get; set; }
        public double[] MissCounts { get; set; }
        public double CheeseNoteCount { get; set; }
        public double[] CheeseLevels { get; set; }
        public double[] CheeseFactors { get; set; }

        public double Length { get; set; }
        public double ApproachRate { get; set; }
        public double OverallDifficulty { get; set; }
        public int TotalObjectCount { get; set; }
        public int HitCircleCount { get; set; }
        public int SliderCount { get; set; }
        public int SpinnerCount { get; set; }
    }
}
