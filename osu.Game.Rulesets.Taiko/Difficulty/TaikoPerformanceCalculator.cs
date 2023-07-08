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

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoPerformanceCalculator : PerformanceCalculator
    {
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private double accuracy;

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
            accuracy = customAccuracy;

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
                difficultyValue *= 1.050;

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= 1.050 * lengthBonus;

            return difficultyValue * Math.Pow(accuracy, 2.0);
        }

        private double computeAccuracyValue(ScoreInfo score, TaikoDifficultyAttributes attributes, bool isConvert)
        {
            if (attributes.GreatHitWindow <= 0)
                return 0;

            double accuracyValue = Math.Pow(60.0 / attributes.GreatHitWindow, 1.1) * Math.Pow(accuracy, 8.0) * Math.Pow(attributes.StarRating, 0.4) * 27.0;

            double lengthBonus = Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));
            accuracyValue *= lengthBonus;

            // Slight HDFL Bonus for accuracy. A clamp is used to prevent against negative values.
            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>) && score.Mods.Any(m => m is ModHidden) && !isConvert)
                accuracyValue *= Math.Max(1.0, 1.1 * lengthBonus);

            return accuracyValue;
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;

        private int totalSuccessfulHits => countGreat + countOk + countMeh;

        private double customAccuracy => totalHits > 0 ? (countGreat * 300 + countOk * 150) / (totalHits * 300.0) : 0;
    }
}
