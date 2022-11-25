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
using MathNet.Numerics.RootFinding;
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
        private double? estimatedDeviation;

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
            estimatedDeviation = computeEstimatedDeviation(score, taikoAttributes);

            // The effectiveMissCount is calculated by gaining a ratio for totalSuccessfulHits and increasing the miss penalty for shorter object counts lower than 1000.
            if (totalSuccessfulHits > 0)
                effectiveMissCount = Math.Max(1.0, 1000.0 / totalSuccessfulHits) * countMiss;

            double multiplier = 1.13;

            if (score.Mods.Any(m => m is ModHidden))
                multiplier *= 1.075;

            if (score.Mods.Any(m => m is ModEasy))
                multiplier *= 0.975;

            double difficultyValue = computeDifficultyValue(score, taikoAttributes);
            double accuracyValue = computeAccuracyValue(score, taikoAttributes);
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
                EstimatedUr = estimatedDeviation * 10,
                Total = totalValue
            };
        }

        private double computeDifficultyValue(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            double difficultyValue = Math.Pow(5 * Math.Max(1.0, attributes.StarRating / 0.115) - 4.0, 2.25) / 1150.0;

            double lengthBonus = 1 + 0.1 * Math.Min(1.0, totalHits / 1500.0);
            difficultyValue *= lengthBonus;

            difficultyValue *= Math.Pow(0.986, effectiveMissCount);

            if (score.Mods.Any(m => m is ModEasy))
                difficultyValue *= 0.985;

            if (score.Mods.Any(m => m is ModHidden))
                difficultyValue *= 1.025;

            if (score.Mods.Any(m => m is ModHardRock))
                difficultyValue *= 1.10;

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= 1.050 * lengthBonus;

            if (estimatedDeviation == null)
                return 0;

            return difficultyValue * Math.Pow(SpecialFunctions.Erf(40 / (Math.Sqrt(2) * estimatedDeviation.Value)), 2.0);
        }

        private double computeAccuracyValue(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            if (attributes.GreatHitWindow <= 0 || estimatedDeviation == null)
                return 0;

            double accuracyValue = Math.Pow(7.5 / estimatedDeviation.Value, 1.1) * Math.Pow(attributes.StarRating / 2.7, 0.8) * 100.0;

            double lengthBonus = Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));

            // Slight HDFL Bonus for accuracy. A clamp is used to prevent against negative values
            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>) && score.Mods.Any(m => m is ModHidden))
                accuracyValue *= Math.Max(1.0, 1.05 * lengthBonus);

            return accuracyValue;
        }

        private double? computeEstimatedDeviation(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            if (totalHits == 0)
                return null;

            // Create a new track to properly calculate the hit windows of 100s and 50s.
            var track = new TrackVirtual(10000);
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            double clockRate = track.Rate;

            double overallDifficulty = (50 - attributes.GreatHitWindow * clockRate) / 3; 
            double goodHitWindow = 0;
            if (overallDifficulty <= 5)
                goodHitWindow = (120 - 8 * overallDifficulty) / clockRate;
            if (overallDifficulty > 5)
                goodHitWindow = 80 - 6 * (overallDifficulty - 5);
            
            double root2 = Math.Sqrt(2);

            double logLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return 1;
                
                double p300 = SpecialFunctions.Erf(attributes.GreatHitWindow / root2 * d);
                double lnP300 = lnErfcApprox(attributes.GreatHitWindow / root2 * d);
                double lnP100 = lnErfcApprox(goodHitWindow / root2 * d);

                double t1 = countGreat * ((attributes.GreatHitWindow * Math.Exp(-Math.Pow(attributes.GreatHitWindow / root2 * d, 2))) / p300);

                double t2a = Math.Exp(Math.Log(goodHitWindow * Math.Exp(-Math.Pow(goodHitWindow / root2 * d, 2))) - logDiff(lnP300, lnP100));
                double t2b = Math.Exp(Math.Log(attributes.GreatHitWindow * Math.Exp(-Math.Pow(attributes.GreatHitWindow / root2 * d, 2))) - logDiff(lnP300, lnP100));
                double t2 = (countOk + 1) * (t2a - t2b);

                double t3 = countMiss * Math.Exp(Math.Log(goodHitWindow * Math.Exp(-Math.Pow(goodHitWindow / root2 * d, 2))) - lnP100);

                return t1 + t2 - t3;
            }

            double root = Brent.FindRootExpand(logLikelihoodGradient, 0.02, 0.3, expandFactor: 2);

            return 1 / root;
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;

        private int totalSuccessfulHits => countGreat + countOk + countMeh;

        double lnErfcApprox(double x)
        {
            if (x <= 5)
                return Math.Log(SpecialFunctions.Erfc(x));
            
            return -Math.Pow(x, 2) - Math.Log(x) - Math.Log(Math.Sqrt(Math.PI));
        }

        double logDiff(double l1, double l2) => l1 + SpecialFunctions.Log1p(-Math.Exp(-(l1 - l2)));
    }
}
