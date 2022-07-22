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

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoPerformanceCalculator : PerformanceCalculator
    {
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;

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

            double multiplier = 1.12; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

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
                Total = totalValue
            };
        }

        private double computeDifficultyValue(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            double difficultyValue = Math.Pow(5 * Math.Max(1.0, attributes.StarRating / 0.115) - 4.0, 2.25) / 1150.0;

            double lengthBonus = 1 + 0.1 * Math.Min(1.0, totalHits / 1500.0);
            difficultyValue *= lengthBonus;

            difficultyValue *= Math.Pow(0.986, countMiss);

            if (score.Mods.Any(m => m is ModEasy))
                difficultyValue *= 0.980;

            if (score.Mods.Any(m => m is ModHidden))
                difficultyValue *= 1.025;

            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= 1.05 * lengthBonus;

            return difficultyValue * Math.Pow(score.Accuracy, 1.5);
        }

        private double computeAccuracyValue(ScoreInfo score, TaikoDifficultyAttributes attributes)
        {
            if (attributes.GreatHitWindow <= 0)
                return 0;

            double accuracyValue = Math.Pow(140.0 / attributes.GreatHitWindow, 1.1) * Math.Pow(score.Accuracy, 12.0) * 27;

            double lengthBonus = Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));
            accuracyValue *= lengthBonus;

            // Slight HDFL Bonus for accuracy.
            if (score.Mods.Any(m => m is ModFlashlight<TaikoHitObject>) && score.Mods.Any(m => m is ModHidden))
                accuracyValue *= 1.10 * lengthBonus;

            return accuracyValue;
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;
    }
}
