// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Strain;
using osu.Game.Rulesets.Mania.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class SunnyStrain : StrainSkill
    {
        private const double final_scaling_factor = 0.975;
        private const double strain_decay_base = .21961997538380362;

        private readonly double[] difficultyPercentilesHigh = { 0.945, 0.935, 0.925, 0.915 };
        private readonly double[] difficultyPercentilesMid = { 0.845, 0.835, 0.825, 0.815 };

        private readonly SunnyStrainData strainData;
        private double currentStrain;
        private double currentNoteCount;
        private double currentLongNoteWeight;

        public SunnyStrain(Mod[] mods, IEnumerable<DifficultyHitObject> difficultyHitObjects, ManiaBeatmap beatmap, FormulaConfig config)
            : base(mods: mods)
        {
            var preprocessor = new SunnyPreprocessor(difficultyHitObjects, beatmap, config);
            strainData = preprocessor.Process();
            currentNoteCount = 0;
            currentLongNoteWeight = 0;
        }

        private double strainDecay(double ms) => Math.Pow(strain_decay_base, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current)
        {
            ManiaDifficultyHitObject prev = (ManiaDifficultyHitObject)current.Previous(0);
            if (prev == null) return 0.0;

            double prevTime = prev.StartTime;
            double deltaMs = Math.Max(0.0, time - prevTime);
            return currentStrain * strainDecay(deltaMs);
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            ManiaDifficultyHitObject maniaCurrent = (ManiaDifficultyHitObject)current;
            double time = maniaCurrent.StartTime;

            double sameColumnValue = SameColumnEvaluator.EvaluateDifficultyAt(time, strainData);
            double crossColumnValue = CrossColumnEvaluator.EvaluateDifficultyAt(time, strainData);
            double pressingValue = PressingIntensityEvaluator.EvaluateDifficultyAt(time, strainData);
            double unevennessValue = UnevennessEvaluator.EvaluateDifficultyAt(time, strainData);
            double releaseValue = ReleaseFactorEvaluator.EvaluateDifficultyAt(time, strainData);

            double noteCount = strainData.SampleFeatureAtTime(time, strainData.LocalNoteCount);
            double activeKeyValue = strainData.SampleFeatureAtTime(time, strainData.ActiveKeyCount);

            double sameColumnClamped = Math.Min(sameColumnValue, 8.0 + 0.85 * sameColumnValue);

            double unevennessPowKey = 1.0;
            if (unevennessValue > 0.0 && activeKeyValue > 0.0)
                unevennessPowKey = Math.Pow(unevennessValue, 3.0 / activeKeyValue);

            double unevennessSameColumnTerm = unevennessPowKey * sameColumnClamped;
            double strain1 = 0.4 * Math.Pow(unevennessSameColumnTerm, 1.5);

            double unevennessPressingReleaseTerm = Math.Pow(unevennessValue, 2.0 / 3.0) * (0.8 * pressingValue + releaseValue * 35.0 / (noteCount + 8.0));
            double strain2 = 0.6 * Math.Pow(unevennessPressingReleaseTerm, 1.5);

            double strainAll = Math.Pow(strain1 + strain2, 2.0 / 3.0);
            double twistAll = (unevennessPowKey * crossColumnValue) / (crossColumnValue + strainAll + 1.0);

            double sqrtStrainAll = Math.Sqrt(strainAll);
            double twistPow = twistAll > 0.0 ? twistAll * Math.Sqrt(twistAll) : 0.0;
            double combinedStrain = 2.7 * sqrtStrainAll * twistPow + strainAll * 0.27;

            currentNoteCount++;

            if (maniaCurrent.EndTime > maniaCurrent.StartTime)
            {
                double dur = Math.Min(maniaCurrent.EndTime - maniaCurrent.StartTime, 1000.0);
                currentLongNoteWeight += 0.5 * dur / 200.0;
            }

            currentStrain = combinedStrain;
            return combinedStrain;
        }

        public override double DifficultyValue()
        {
            var peaks = GetCurrentStrainPeaks().Where(p => p > 0);
            double[] sorted = peaks.OrderDescending().ToArray();
            Array.Sort(sorted);

            double highPercentileMean = DifficultyValueUtils.CalculatePercentileMean(sorted, difficultyPercentilesHigh);
            double midPercentileMean = DifficultyValueUtils.CalculatePercentileMean(sorted, difficultyPercentilesMid);
            double powerMean = DifficultyValueUtils.CalculatePowerMean(sorted, 5.0);

            double rawDifficulty = 0.25 * (0.88 * highPercentileMean) +
                                   0.20 * (0.94 * midPercentileMean) +
                                   0.55 * powerMean;

            return applyFinalScaling(rawDifficulty);
        }

        private double applyFinalScaling(double rawDifficulty)
        {
            double scaled = rawDifficulty;

            // Progressive note count for potential pp counter fix (regular notes + long note weight)
            double totalCurrentNotes = currentNoteCount + currentLongNoteWeight;
            scaled *= totalCurrentNotes / (totalCurrentNotes + 60.0);

            if (scaled > strainData.Config.rescaleHighThreshold)
            {
                scaled = strainData.Config.rescaleHighThreshold + (scaled - strainData.Config.rescaleHighThreshold) / strainData.Config.rescaleHighFactor;
            }

            return scaled * final_scaling_factor;
        }

        /*
         private double applyFinalScaling(double rawDifficulty)
         {
            double scaled = rawDifficulty;
            double totalNotes = strainData.AllNotes.Length;

            for (int i = 0; i < strainData.LongNotes.Length; i++)
            {
                var ln = strainData.LongNotes[i];
                double dur = Math.Min(ln.EndTime - ln.StartTime, 1000);
                totalNotes += 0.5 * dur / 200.0;
            }

            scaled *= totalNotes / (totalNotes + 60.0);

            if (scaled > strainData.Config.rescaleHighThreshold)
            {
                scaled = strainData.Config.rescaleHighThreshold + (scaled - strainData.Config.rescaleHighThreshold) / strainData.Config.rescaleHighFactor;
            }

            return scaled * final_scaling_factor;
         }
         */
    }

    // TEMPORARY ONLY FOR TESTING
    public class FormulaConfig
    {
        public double rescaleHighThreshold = 9.080318931478196;
        public double rescaleHighFactor = 1.3276270825877536;
        public double hitLeniencyBase = 0.3645051554705115;
        public double hitLeniencyOdMultiplier = 3.5246842251528054;
        public double hitLeniencyOdBase = 56.56596897139995;
        public double smoothingWindowMs = 570.8536620240726;
        public double accuracySmoothingWindowMs = 453.97171661313587;
        public double columnActivityWindowMs = 83.62480391692215;
        public double keyUsageWindowMs = 338.6299547206798;
        public double jackNerfCoefficient = 0.7147442967828707;
        public double jackNerfBase = 3.606794925409644;
        public double jackNerfPower = -2.3810559991711413;
        public double streamBoostMinRatio = 172.4455803487851;
        public double streamBoostMaxRatio = 282.44785133399847;
        public double streamBoostCoefficient = 9.072350572502293E-7;
    }
}
