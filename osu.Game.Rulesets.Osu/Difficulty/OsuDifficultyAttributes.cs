// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double TapSr { get; set; }
        public double TapDiff { get; set; }
        public double StreamNoteCount { get; set; }
        public double MashTapDiff { get; set; }

        public double FingerControlSr { get; set; }
        public double FingerControlDiff { get; set; }

        public double AimSr { get; set; }
        public double AimDiff { get; set; }
        public double AimHiddenFactor { get; set; }
        public double[] ComboTps { get; set; }
        public double[] MissTps { get; set; }
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
