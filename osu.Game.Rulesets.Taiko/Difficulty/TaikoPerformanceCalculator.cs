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
        protected new TaikoDifficultyAttributes Attributes => (TaikoDifficultyAttributes)base.Attributes;

        private Mod[] mods;
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;

        public TaikoPerformanceCalculator(Ruleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public override PerformanceAttributes Calculate()
        {
            mods = Score.Mods;
            countGreat = Score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = Score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetValueOrDefault(HitResult.Miss);

            double multiplier = 1.1; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is ModNoFail))
                multiplier *= 0.90;

            if (mods.Any(m => m is ModHidden))
                multiplier *= 1.10;

            double difficultyValue = computeDifficultyValue();
            double accuracyValue = computeAccuracyValue();
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

        private double computeDifficultyValue()
        {
            double difficultyValue = Math.Pow(5.0 * Math.Max(1.0, Attributes.StarRating / 0.0075) - 4.0, 2.0) / 100000.0;

            double lengthBonus = 1 + 0.1 * Math.Min(1.0, totalHits / 1500.0);
            difficultyValue *= lengthBonus;

            difficultyValue *= Math.Pow(0.985, countMiss);

            if (mods.Any(m => m is ModHidden))
                difficultyValue *= 1.025;

            if (mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                difficultyValue *= 1.05 * lengthBonus;

            return difficultyValue * Score.Accuracy;
        }

        private double computeAccuracyValue()
        {
            if (Attributes.GreatHitWindow <= 0)
                return 0;

            double accValue = Math.Pow(150.0 / Attributes.GreatHitWindow, 1.1) * Math.Pow(Score.Accuracy, 15) * 22.0;

            // Bonus for many objects - it's harder to keep good accuracy up for longer
            return accValue * Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;
    }
}
