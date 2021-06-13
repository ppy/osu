// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double TapSr;
        public double TapDiff;
        public double StreamNoteCount;
        public double MashTapDiff;

        public double FingerControlSr;
        public double FingerControlDiff;

        public double AimSr;
        public double AimDiff;
        public double AimHiddenFactor;
        public double[] ComboTps;
        public double[] MissTps;
        public double[] MissCounts;
        public double CheeseNoteCount;
        public double[] CheeseLevels;
        public double[] CheeseFactors;

        public double Length;
        public double ApproachRate;
        public double OverallDifficulty;
        public int TotalObjectCount;
        public int HitCircleCount;
        public int SliderCount;
        public int SpinnerCount;
    }
}
