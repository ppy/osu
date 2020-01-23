// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double TapSR;
        public double TapDiff;
        public double StreamNoteCount;
        public double[] MashLevels;
        public double[] TapSkills;

        public double FingerControlSR;
        public double FingerControlDiff;

        public double AimSR;
        public double AimDiff;
        public double AimHiddenFactor;
        public double[] ComboTPs;
        public double[] MissTPs;
        public double[] MissCounts;
        public double CheeseNoteCount;
        public double[] CheeseLevels;
        public double[] CheeseFactors;

        public double Length;
        public double ApproachRate;
        public double OverallDifficulty;
        public int MaxCombo;
    }
}
