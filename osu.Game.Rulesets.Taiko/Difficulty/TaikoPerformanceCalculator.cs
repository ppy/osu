// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Scoring;
using MathNet.Numerics;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;

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
            bool isConvert = score.BeatmapInfo.Ruleset.OnlineID != 1;

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

            double accuracyValue = Math.Pow(60 / estimatedUr.Value, 1.1) * Math.Pow(attributes.StarRating, 0.4) * 100.0;

            double lengthBonus = Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));

            // Slight HDFL Bonus for accuracy. A clamp is used to prevent against negative values.
            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>) && score.Mods.Any(m => m is ModHidden) && !isConvert)
                accuracyValue *= Math.Max(1.0, 1.05 * lengthBonus);

            return accuracyValue;
        }

        /// <summary>
        /// Estimates the player's tap deviation based on the OD, number of objects, and number of 300s, 100s, and misses,
        /// assuming the player's mean hit error is 0. The estimation is consistent in that two SS scores on the same map with the same settings
        /// will always return the same deviation. See: https://www.desmos.com/calculator/qlr946netu
        /// </summary>
        private double? computeEstimatedUr(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0 || attributes.GreatHitWindow == 0)
                return null;

            // Create a new track to properly calculate the hit window of 100s.
            var track = new TrackVirtual(10000);
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            double clockRate = track.Rate;

            double overallDifficulty = (50 - attributes.GreatHitWindow * clockRate) / 3;
            double h300 = attributes.GreatHitWindow;
            double h100 = overallDifficulty <= 5 ? (120 - 8 * overallDifficulty) / clockRate : (80 - 6 * (overallDifficulty - 5)) / clockRate;

            // Returns the likelihood of a deviation resulting in the score's hit judgements. The peak of the curve is the most likely deviation.
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

        // There is a numerical approximation to increase how far you can calculate Erfc(x).
        private double logErfcApprox(double x) => x <= 5 ? Math.Log(SpecialFunctions.Erfc(x)) : -Math.Pow(x, 2) - Math.Log(x) - Math.Log(Math.Sqrt(Math.PI));

        // Log rules make subtraction of the non-log value non-trivial, this method simply subtracts the base value of 2 logs.
        private double logDiff(double firstLog, double secondLog)
        {
            double maxVal = Math.Max(firstLog, secondLog);

            // Avoid negative infinity - negative infinity (NaN) by checking if the higher value is negative infinity.
            // Shouldn't ever happen, but good for redundancy purposes.
            if (double.IsNegativeInfinity(maxVal))
            {
                return maxVal;
            }

            return firstLog + SpecialFunctions.Log1p(-Math.Exp(-(firstLog - secondLog)));
        }
    }
}
