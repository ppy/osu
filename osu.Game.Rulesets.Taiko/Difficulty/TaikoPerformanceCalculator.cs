﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        private double effectiveMissCount;

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

            estimatedUnstableRate = computeDeviationUpperBound() * 10;

            // Effective miss count is calculated by raising the fraction of hits missed to a power based on the map's consistency factor.
            // This is because in less consistently difficult maps, each miss removes more of the map's total difficulty.
            effectiveMissCount = totalHits * Math.Pow(
                (double)countMiss / totalHits,
                Math.Pow(taikoAttributes.ConsistencyFactor, 0.2)
            );

            // Converts are detected and omitted from mod-specific bonuses due to the scope of current difficulty calculation.
            bool isConvert = score.BeatmapInfo!.Ruleset.OnlineID != 1;

            double difficultyValue = computeDifficultyValue(score, taikoAttributes, isConvert) * 1.08;
            double accuracyValue = computeAccuracyValue(score, taikoAttributes, isConvert) * 1.1;

            return new TaikoPerformanceAttributes
            {
                Difficulty = difficultyValue,
                Accuracy = accuracyValue,
                EffectiveMissCount = effectiveMissCount,
                EstimatedUnstableRate = estimatedUnstableRate,
                Total = difficultyValue + accuracyValue
            };
        }

        private double computeDifficultyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert)
        {
            double baseDifficulty = 5 * Math.Max(1.0, attributes.StarRating / 0.110) - 4.0;
            double difficultyValue = Math.Min(Math.Pow(baseDifficulty, 3) / 69052.51, Math.Pow(baseDifficulty, 2.25) / 1250.0);

            difficultyValue *= 1 + 0.10 * Math.Max(0, attributes.StarRating - 10);

            // Applies a bonus to maps with more total difficulty, calculating this with a map's total hits and consistency factor.
            double totalDifficultHits = totalHits * Math.Pow(attributes.ConsistencyFactor, 0.5);
            double lengthBonus = 1 + 0.25 * totalDifficultHits / (totalDifficultHits + 4000);
            difficultyValue *= lengthBonus;

            // Scales miss penalty by the total hits of a map, making misses more punishing on maps with fewer objects.
            double missPenalty = Math.Pow(0.5, 30.0 / totalHits);
            difficultyValue *= Math.Pow(missPenalty, effectiveMissCount);

            if (score.Mods.Any(m => m is ModHidden))
                difficultyValue *= (isConvert) ? 1.025 : 1.1;

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= Math.Max(1, 1.050 - Math.Min(attributes.MonoStaminaFactor / 50, 1) * lengthBonus);

            if (estimatedUnstableRate == null)
                return 0;

            // Scale accuracy more harshly on nearly-completely mono (single coloured) speed maps.
            double accScalingExponent = 2 + attributes.MonoStaminaFactor;
            double accScalingShift = 500 - 100 * (attributes.MonoStaminaFactor * 3);

            return difficultyValue * Math.Pow(DifficultyCalculationUtils.Erf(accScalingShift / (Math.Sqrt(2) * estimatedUnstableRate.Value)), accScalingExponent);
        }

        private double computeAccuracyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert)
        {
            if (greatHitWindow <= 0 || estimatedUnstableRate == null)
                return 0;

            double accuracyValue = Math.Pow(70 / estimatedUnstableRate.Value, 1.1) * Math.Pow(attributes.StarRating, 0.4) * 100.0;

            if (score.Mods.Any(m => m is ModHidden) && !isConvert)
                accuracyValue *= 1.075;

            // Applies a bonus to maps with more total difficulty, calculating this with a map's total hits and consistency factor.
            double totalDifficultHits = totalHits * Math.Pow(attributes.ConsistencyFactor, 0.5);
            double lengthBonus = 1 + 0.4 * totalDifficultHits / (totalDifficultHits + 4000);
            accuracyValue *= lengthBonus;

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
        private double? computeDeviationUpperBound()
        {
            if (countGreat == 0 || greatHitWindow <= 0)
                return null;

            const double z = 2.32634787404; // 99% critical value for the normal distribution (one-tailed).

            double n = totalHits;

            // Proportion of greats hit.
            double p = countGreat / n;

            // We can be 99% confident that p is at least this value.
            double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

            // We can be 99% confident that the deviation is not higher than:
            return greatHitWindow / (Math.Sqrt(2) * DifficultyCalculationUtils.ErfInv(pLowerBound));
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;

        private int totalSuccessfulHits => countGreat + countOk + countMeh;
    }
}
