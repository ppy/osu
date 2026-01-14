// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainSkill
    {
        // Difficulty calculation weights
        private const double high_percentile_weight = 0.22; // 0.25 * 0.88
        private const double mid_percentile_weight = 0.188; // 0.20 * 0.94
        private const double power_mean_weight = 0.55;

        private readonly double[] difficultyPercentilesHigh = { 0.945, 0.935, 0.925, 0.915 };
        private readonly double[] difficultyPercentilesMid = { 0.845, 0.835, 0.825, 0.815 };

        private const double rescale_high_threshold = 9.0;
        private const double rescale_high_factor = 1.2;

        private double currentStrain;
        private double currentNoteCount;
        private double currentLongNoteWeight;

        public Strain(Mod[] mods)
            : base(mods: mods)
        {
        }

        // Just for visualization
        protected override double CalculateInitialStrain(double time, DifficultyHitObject current)
        {
            return currentStrain;
        }

        public override double DifficultyValue()
        {
            double[] sorted = ObjectStrains.Where(s => s > 0).ToArray();
            if (sorted.Length == 0) return 0.0;

            Array.Sort(sorted);

            double highPercentileMean = DifficultyValueUtils.CalculatePercentileMean(sorted, difficultyPercentilesHigh);
            double midPercentileMean = DifficultyValueUtils.CalculatePercentileMean(sorted, difficultyPercentilesMid);
            double powerMean = DifficultyValueUtils.CalculatePowerMean(sorted, 5.0);

            double rawDifficulty = high_percentile_weight * highPercentileMean +
                                   mid_percentile_weight * midPercentileMean +
                                   power_mean_weight * powerMean;

            double weightedNoteCount = getWeightedNoteCount();

            // Short map nerf
            double scaled = rawDifficulty * weightedNoteCount / (weightedNoteCount + 60.0);

            // Adjust high-end star ratings slightly
            if (scaled > rescale_high_threshold)
            {
                scaled = rescale_high_threshold + (scaled - rescale_high_threshold) / rescale_high_factor;
            }

            return scaled;
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            ManiaDifficultyHitObject maniaCurrent = (ManiaDifficultyHitObject)current;
            ManiaDifficultyHitObject prev = (ManiaDifficultyHitObject)current.Previous(0);

            currentNoteCount++;

            if (maniaCurrent.IsLong)
            {
                double longNoteDuration = Math.Min(maniaCurrent.EndTime - maniaCurrent.StartTime, 1000.0);
                currentLongNoteWeight += 0.5 * longNoteDuration / 200.0;
            }

            if (prev != null && prev.StartTime == maniaCurrent.StartTime)
                return currentStrain;

            currentStrain = StrainEvaluator.EvaluateDifficultyOf(maniaCurrent);
            return currentStrain;
        }

        private double getWeightedNoteCount()
        {
            return currentNoteCount + currentLongNoteWeight;
        }
    }
}
