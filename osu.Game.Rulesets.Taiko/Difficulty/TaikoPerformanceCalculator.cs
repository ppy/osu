// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Scoring;
using MathNet.Numerics;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoPerformanceCalculator : PerformanceCalculator
    {
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private double? estimatedUr;

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
            estimatedUr = computeEstimatedUr(score, taikoAttributes);

            // The effectiveMissCount is calculated by gaining a ratio for totalSuccessfulHits and increasing the miss penalty for shorter object counts lower than 1000.
            if (totalSuccessfulHits > 0)
                effectiveMissCount = Math.Max(1.0, 1000.0 / totalSuccessfulHits) * countMiss;

            // TODO: The detection of rulesets is temporary until the leftover old skills have been reworked.
            bool isConvert = score.BeatmapInfo!.Ruleset.OnlineID != 1;

            double multiplier = 1.13;

            if (score.Mods.Any(m => m is ModHidden))
                multiplier *= 1.075;

            if (score.Mods.Any(m => m is ModEasy))
                multiplier *= 0.975;

            double difficultyValue = computeDifficultyValue(score, taikoAttributes, isConvert);
            double accuracyValue = computeAccuracyValue(score, taikoAttributes, isConvert);
            double totalValue =
                Math.Pow(
                    Math.Pow(difficultyValue, 1.1) +
                    Math.Pow(accuracyValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            return new TaikoPerformanceAttributes
            {
                Difficulty = difficultyValue,
                Accuracy = accuracyValue,
                EffectiveMissCount = effectiveMissCount,
                EstimatedUr = estimatedUr,
                Total = totalValue
            };
        }

        private double computeDifficultyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert)
        {
            double difficultyValue = Math.Pow(5 * Math.Max(1.0, attributes.StarRating / 0.115) - 4.0, 2.25) / 1150.0;

            double lengthBonus = 1 + 0.1 * Math.Min(1.0, totalHits / 1500.0);
            difficultyValue *= lengthBonus;

            difficultyValue *= Math.Pow(0.986, effectiveMissCount);

            if (score.Mods.Any(m => m is ModEasy))
                difficultyValue *= 0.985;

            if (score.Mods.Any(m => m is ModHidden) && !isConvert)
                difficultyValue *= 1.025;

            if (score.Mods.Any(m => m is ModHardRock))
                difficultyValue *= 1.10;

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= 1.050 * lengthBonus;

            if (estimatedUr == null)
                return 0;

            return difficultyValue * Math.Pow(SpecialFunctions.Erf(400 / (Math.Sqrt(2) * estimatedUr.Value)), 2.0);
        }

        private double computeAccuracyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert)
        {
            if (attributes.GreatHitWindow <= 0 || estimatedUr == null)
                return 0;

            double accuracyValue = Math.Pow(70 / estimatedUr.Value, 1.1) * Math.Pow(attributes.StarRating, 0.4) * 110.0;

            double lengthBonus = Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));

            // Slight HDFL Bonus for accuracy. A clamp is used to prevent against negative values.
            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>) && score.Mods.Any(m => m is ModHidden) && !isConvert)
                accuracyValue *= Math.Max(1.0, 1.05 * lengthBonus);

            return accuracyValue;
        }

        /// <summary>
        /// Calculates the tap deviation for a player using the OD, object count, and scores of 300s, 100s, and misses, with an assumed mean hit error of 0.
        /// Consistency is ensured as identical SS scores on the same map and settings yield the same deviation.
        /// </summary>
        private double? computeEstimatedUr(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0 || attributes.GreatHitWindow <= 0)
                return null;

            double h300 = attributes.GreatHitWindow;
            double h100 = attributes.OkHitWindow;

            // Determines the probability of a deviation leading to the score's hit evaluations. The curve's apex represents the most probable deviation.
            double likelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                double p300 = logDiff(0, logPcHit(h300, d));
                double p100 = logDiff(logPcHit(h300, d), logPcHit(h100, d));
                double p0 = logPcHit(h100, d);

                double gradient = Math.Exp(
                    (countGreat * p300
                     + (countOk + 0.5) * p100
                     + countMiss * p0) / totalHits
                );

                return -gradient;
            }

            double deviation = FindMinimum.OfScalarFunction(likelihoodGradient, 30);

            return deviation * 10;
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;

        private int totalSuccessfulHits => countGreat + countOk + countMeh;

        private double logPcHit(double x, double deviation) => logErfcApprox(x / (deviation * Math.Sqrt(2)));

        // Utilises a numerical approximation to extend the computation range of ln(erfc(x)).
        private double logErfcApprox(double x) => x <= 5
            ? Math.Log(SpecialFunctions.Erfc(x))
            : -Math.Pow(x, 2) - Math.Log(x * Math.Sqrt(Math.PI)); // https://www.desmos.com/calculator/kdbxwxgf01

        // Subtracts the base value of two logs, circumventing log rules that typically complicate subtraction of non-logarithmic values.
        private double logDiff(double firstLog, double secondLog)
        {
            double maxVal = Math.Max(firstLog, secondLog);

            // To avoid a NaN result, a check is performed to prevent subtraction of two negative infinity values.
            if (double.IsNegativeInfinity(maxVal))
            {
                return maxVal;
            }

            return firstLog + SpecialFunctions.Log1p(-Math.Exp(-(firstLog - secondLog)));
        }
    }
}
