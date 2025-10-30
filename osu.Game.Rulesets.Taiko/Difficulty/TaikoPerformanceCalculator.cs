// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Scoring;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoPerformanceCalculator : PerformanceCalculator
    {
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private double? estimatedUnstableRate;

        private double clockRate;
        private double greatHitWindow;

        private double totalDifficultHits;

        public TaikoPerformanceCalculator()
            : base(new TaikoRuleset())
        {
        }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var taikoAttributes = (TaikoDifficultyAttributes)attributes;

            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);

            clockRate = ModUtils.CalculateRateWithMods(score.Mods);

            var difficulty = score.BeatmapInfo!.Difficulty.Clone();

            score.Mods.OfType<IApplicableToDifficulty>().ForEach(m => m.ApplyToDifficulty(difficulty));

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(difficulty.OverallDifficulty);

            greatHitWindow = hitWindows.WindowFor(HitResult.Great) / clockRate;

            estimatedUnstableRate = (countGreat == 0 || greatHitWindow <= 0)
                ? null
                : computeDeviationUpperBound(countGreat / (double)totalHits) * 10;

            // Total difficult hits measures the total difficulty of a map based on its consistency factor.
            totalDifficultHits = totalHits * taikoAttributes.ConsistencyFactor;

            // Converts and the classic mod are detected and omitted from mod-specific bonuses due to the scope of current difficulty calculation.
            bool isConvert = score.BeatmapInfo!.Ruleset.OnlineID != 1;
            bool isClassic = score.Mods.Any(m => m is ModClassic);

            double difficultyValue = computeDifficultyValue(score, taikoAttributes, isConvert, isClassic) * 1.08;
            double accuracyValue = computeAccuracyValue(score, taikoAttributes, isConvert) * 1.1;

            return new TaikoPerformanceAttributes
            {
                Difficulty = difficultyValue,
                Accuracy = accuracyValue,
                EstimatedUnstableRate = estimatedUnstableRate,
                Total = difficultyValue + accuracyValue
            };
        }

        private double computeDifficultyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert, bool isClassic)
        {
            if (estimatedUnstableRate == null || totalDifficultHits == 0)
                return 0;

            // The estimated unstable rate for 100% accuracy, at which all rhythm difficulty has been played successfully.
            double rhythmExpectedUnstableRate = computeDeviationUpperBound(1.0) * 10;

            // The unstable rate at which it can be assumed all rhythm difficulty has been ignored.
            // 0.8 represents 80% of total hits being greats, or 90% accuracy in-game
            double rhythmMaximumUnstableRate = computeDeviationUpperBound(0.8) * 10;

            // The fraction of star rating made up by rhythm difficulty, normalised to represent rhythm's perceived contribution to star rating.
            double rhythmFactor = DifficultyCalculationUtils.ReverseLerp(attributes.RhythmDifficulty / attributes.StarRating, 0.15, 0.4);

            // A penalty removing improperly played rhythm difficulty from star rating based on estimated unstable rate.
            double rhythmPenalty = 1 - DifficultyCalculationUtils.Logistic(
                estimatedUnstableRate.Value,
                midpointOffset: (rhythmExpectedUnstableRate + rhythmMaximumUnstableRate) / 2,
                multiplier: 10 / (rhythmMaximumUnstableRate - rhythmExpectedUnstableRate),
                maxValue: 0.25 * Math.Pow(rhythmFactor, 3)
            );

            double baseDifficulty = 5 * Math.Max(1.0, attributes.StarRating * rhythmPenalty / 0.110) - 4.0;
            double difficultyValue = Math.Min(Math.Pow(baseDifficulty, 3) / 69052.51, Math.Pow(baseDifficulty, 2.25) / 1250.0);

            difficultyValue *= 1 + 0.10 * Math.Max(0, attributes.StarRating - 10);

            // Applies a bonus to maps with more total difficulty.
            double lengthBonus = 1 + 0.25 * totalDifficultHits / (totalDifficultHits + 4000);
            difficultyValue *= lengthBonus;

            // Scales miss penalty by the total difficult hits of a map, making misses more punishing on maps with less total difficulty.
            double missPenalty = 0.97 + 0.03 * totalDifficultHits / (totalDifficultHits + 1500);
            difficultyValue *= Math.Pow(missPenalty, countMiss);

            if (score.Mods.Any(m => m is ModHidden))
            {
                double hiddenBonus = isConvert ? 0.025 : 0.1;

                // Hidden+flashlight plays are excluded from reading-based penalties to hidden.
                if (!score.Mods.Any(m => m is ModFlashlight))
                {
                    // A penalty is applied to the bonus for hidden on non-classic scores, as the playfield can be made wider to make fast reading easier.
                    if (!isClassic)
                        hiddenBonus *= 0.2;

                    // A penalty is applied to classic easy+hidden scores, as notes disappear later making fast reading easier.
                    if (score.Mods.Any(m => m is ModEasy) && isClassic)
                        hiddenBonus *= 0.5;
                }

                difficultyValue *= 1 + hiddenBonus;
            }

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= Math.Max(1, 1.050 - Math.Min(attributes.MonoStaminaFactor / 50, 1) * lengthBonus);

            // Scale accuracy more harshly on nearly-completely mono (single coloured) speed maps.
            double monoAccScalingExponent = 2 + attributes.MonoStaminaFactor;
            double monoAccScalingShift = 500 - 100 * (attributes.MonoStaminaFactor * 3);

            return difficultyValue * Math.Pow(DifficultyCalculationUtils.Erf(monoAccScalingShift / (Math.Sqrt(2) * estimatedUnstableRate.Value)), monoAccScalingExponent);
        }

        private double computeAccuracyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert)
        {
            if (greatHitWindow <= 0 || estimatedUnstableRate == null)
                return 0;

            double accuracyValue = 470 * Math.Pow(0.9885, estimatedUnstableRate.Value);

            // Scales up the bonus for lower unstable rate as star rating increases.
            accuracyValue *= 1 + Math.Pow(50 / estimatedUnstableRate.Value, 2) * Math.Pow(attributes.StarRating, 2.8) / 600;

            if (score.Mods.Any(m => m is ModHidden) && !isConvert)
                accuracyValue *= 1.075;

            // Applies a bonus to maps with more total difficulty, calculating this with a map's total hits and consistency factor.
            accuracyValue *= 1 + 0.3 * totalDifficultHits / (totalDifficultHits + 4000);

            // Applies a bonus to maps with more total memory required with HDFL.
            double memoryLengthBonus = Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>) && score.Mods.Any(m => m is ModHidden) && !isConvert)
                accuracyValue *= Math.Max(1.0, 1.05 * memoryLengthBonus);

            return accuracyValue;
        }

        /// <summary>
        /// Computes an upper bound on the player's tap deviation based on the OD, number of circles and sliders,
        /// and the hit judgements, assuming the player's mean hit error is 0. The estimation is consistent in that
        /// two SS scores on the same map with the same settings will always return the same deviation.
        /// </summary>
        private double computeDeviationUpperBound(double accuracy)
        {
            const double z = 2.32634787404; // 99% critical value for the normal distribution (one-tailed).

            double n = totalHits;

            // Proportion of greats hit.
            double p = accuracy;

            // We can be 99% confident that p is at least this value.
            double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

            // We can be 99% confident that the deviation is not higher than:
            return greatHitWindow / (Math.Sqrt(2) * DifficultyCalculationUtils.ErfInv(pLowerBound));
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;

        private int totalSuccessfulHits => countGreat + countOk + countMeh;
    }
}
