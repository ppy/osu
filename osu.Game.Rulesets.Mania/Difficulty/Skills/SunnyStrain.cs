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
        private const double strain_decay_base = .20143474157245744;

        private readonly double[] difficultyPercentilesHigh = { 0.945, 0.935, 0.925, 0.915 };
        private readonly double[] difficultyPercentilesMid = { 0.845, 0.835, 0.825, 0.815 };

        private readonly SunnyStrainData strainData;
        private double currentStrain;
        private double currentNoteCount;
        private double currentLongNoteWeight;

        public SunnyStrain(Mod[] mods, IEnumerable<DifficultyHitObject> difficultyHitObjects, ManiaBeatmap beatmap, FormulaConfig config)
            : base(mods: mods)
        {
            // Process arrays here
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
            ManiaDifficultyHitObject prev = (ManiaDifficultyHitObject)current.Previous(0);

            currentNoteCount++;

            if (maniaCurrent.EndTime > maniaCurrent.StartTime)
            {
                double longNoteDuration = Math.Min(maniaCurrent.EndTime - maniaCurrent.StartTime, 1000.0);
                currentLongNoteWeight += 0.5 * longNoteDuration / 200.0;
            }

            // If this note shares the same StartTime as the previous (basically a chord) then we'll reuse the currentStrain
            if (prev != null && prev.StartTime == maniaCurrent.StartTime)
            {
                return currentStrain;
            }

            double currentTime = maniaCurrent.StartTime;

            double sameColumnDifficulty = SameColumnEvaluator.EvaluateDifficultyAt(currentTime, strainData);
            double crossColumnDifficulty = CrossColumnEvaluator.EvaluateDifficultyAt(currentTime, strainData);
            double pressingIntensityDifficulty = PressingIntensityEvaluator.EvaluateDifficultyAt(currentTime, strainData);
            double unevennessDifficulty = UnevennessEvaluator.EvaluateDifficultyAt(currentTime, strainData);
            double releaseDifficulty = ReleaseFactorEvaluator.EvaluateDifficultyAt(currentTime, strainData);

            double localNoteCount = strainData.SampleFeatureAtTime(currentTime, strainData.LocalNoteCount);
            double activeKeyCount = strainData.SampleFeatureAtTime(currentTime, strainData.ActiveKeyCount);

            double clampedSameColumnDifficulty = Math.Min(sameColumnDifficulty, 8.0 + 0.85 * sameColumnDifficulty);

            double unevennessKeyAdjustment = 1.0;
            if (unevennessDifficulty > 0.0 && activeKeyCount > 0.0)
                unevennessKeyAdjustment = Math.Pow(unevennessDifficulty, 3.0 / activeKeyCount);

            double unevennessSameColumnComponent = unevennessKeyAdjustment * clampedSameColumnDifficulty;
            double firstStrainComponent = 0.4 * Math.Pow(unevennessSameColumnComponent, 1.5);

            double unevennessPressingReleaseComponent = Math.Pow(unevennessDifficulty, 2.0 / 3.0) * (0.8 * pressingIntensityDifficulty + releaseDifficulty * 35.0 / (localNoteCount + 8.0));
            double secondStrainComponent = 0.6 * Math.Pow(unevennessPressingReleaseComponent, 1.5);

            double totalStrainDifficulty = Math.Pow(firstStrainComponent + secondStrainComponent, 2.0 / 3.0);

            double twistComponent = (unevennessKeyAdjustment * crossColumnDifficulty) / (crossColumnDifficulty + totalStrainDifficulty + 1.0);

            double poweredTwistComponent = twistComponent > 0.0 ? twistComponent * Math.Sqrt(twistComponent) : 0.0;
            double finalCombinedStrain = 2.7 * Math.Sqrt(totalStrainDifficulty) * poweredTwistComponent + totalStrainDifficulty * 0.27;

            currentStrain = finalCombinedStrain;

            //Console.WriteLine($"SR: {DifficultyValue()} Time: {current.StartTime} Current Strain: {currentStrain}");
            return finalCombinedStrain;
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
        public double rescaleHighThreshold = 10.01191947310021;
        public double rescaleHighFactor = 1.4413495576620445;
        public double hitLeniencyBase = 0.3274102106755597;
        public double hitLeniencyOdMultiplier = 3.4089356422856554;
        public double hitLeniencyOdBase = 55.07787194224247;
        public double smoothingWindowMs = 493.6941401846966;
        public double accuracySmoothingWindowMs = 500.0;
        public double columnActivityWindowMs = 246.75167259941304;
        public double keyUsageWindowMs =  323.9887868867879;
        public double jackNerfCoefficient = 0.46404885543338015;
        public double jackNerfBase = 18.414285011810225;
        public double jackNerfPower = -34.692933993910664;
        public double streamBoostMinRatio = 169.97179025061487;
        public double streamBoostMaxRatio = 382.02728653035257;
        public double streamBoostCoefficient = 8.090697350532603E-8;
    }
}
