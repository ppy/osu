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
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;
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

            /*Console.WriteLine($"time={currentTime}, sameColumn={sameColumnDifficulty}, crossColumn={crossColumnDifficulty}, "
                              + $"pressingIntensity={pressingIntensityDifficulty}, unevenessDifficulty={unevennessDifficulty}, releaseDifficulty={releaseDifficulty}, localNoteCount={localNoteCount}, "
                              + $"activeKeyCount={activeKeyCount} \n");*/

            double clampedSameColumnDifficulty = Math.Min(sameColumnDifficulty, 8.0 + 0.85 * sameColumnDifficulty);

            // Adjust unevenness impact based on how many keys are active
            // More keys = unevenness matters less per key
            double unevennessKeyAdjustment = 1.0;
            if (unevennessDifficulty > 0.0 && activeKeyCount > 0.0)
                unevennessKeyAdjustment = Math.Pow(unevennessDifficulty, 3.0 / activeKeyCount);

            // Combine unevenness with same-column difficulty (how uneven jacks/streams are)
            double unevennessSameColumnComponent = unevennessKeyAdjustment * clampedSameColumnDifficulty;
            double firstStrainComponent = 0.4 * Math.Pow(unevennessSameColumnComponent, 1.5);

            // Combine unevenness with pressing intensity and release difficulty
            double unevennessPressingReleaseComponent = Math.Pow(unevennessDifficulty, 2.0 / 3.0) * (0.8 * pressingIntensityDifficulty + releaseDifficulty * 35.0 / (localNoteCount + 8.0));
            double secondStrainComponent = 0.6 * Math.Pow(unevennessPressingReleaseComponent, 1.5);

            // Main strain difficulty combining both components
            double totalStrainDifficulty = Math.Pow(firstStrainComponent + secondStrainComponent, 2.0 / 3.0);

            // How much cross-column coordination adds to the difficulty
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
    }

    // TEMPORARY ONLY FOR TESTING
    public class FormulaConfig
    {
        public double rescaleHighThreshold = 9.648018295800334;
        public double rescaleHighFactor = 1.4441598782471803;
        public double hitLeniencyBase = 0.3310181856698675;
        public double hitLeniencyOdMultiplier = 3.360810802810689;
        public double hitLeniencyOdBase = 56.59723759581402;
        public double smoothingWindowMs = 490.74808264043077;
        public double accuracySmoothingWindowMs = 383.2749819546263;
        public double columnActivityWindowMs = 231.11564703753132;
        public double keyUsageWindowMs = 310.92348654941304;
        public double jackNerfCoefficient = 0.47020623513898313;
        public double jackNerfBase = 18.441418584326687;
        public double jackNerfPower = -33.72322163074505;
        public double streamBoostMinRatio = 172.41458658810265;
        public double streamBoostMaxRatio = 384.7187521409616;
        public double streamBoostCoefficient = 6.58242983168459E-8;
    }
}
