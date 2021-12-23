// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaPerformanceCalculator : PerformanceCalculator
    {
        protected new ManiaDifficultyAttributes Attributes => (ManiaDifficultyAttributes)base.Attributes;

        private Mod[] mods;

        // Score after being scaled by non-difficulty-increasing mods
        private double scaledScore;

        private int countPerfect;
        private int countGreat;
        private int countGood;
        private int countOk;
        private int countMeh;
        private int countMiss;

        public ManiaPerformanceCalculator(Ruleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public override PerformanceAttributes Calculate()
        {
            mods = Score.Mods;
            scaledScore = Score.TotalScore;
            countPerfect = Score.Statistics.GetValueOrDefault(HitResult.Perfect);
            countGreat = Score.Statistics.GetValueOrDefault(HitResult.Great);
            countGood = Score.Statistics.GetValueOrDefault(HitResult.Good);
            countOk = Score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetValueOrDefault(HitResult.Miss);

            IEnumerable<Mod> scoreIncreaseMods = Ruleset.GetModsFor(ModType.DifficultyIncrease);

            double scoreMultiplier = 1.0;
            foreach (var m in mods.Where(m => !scoreIncreaseMods.Contains(m)))
                scoreMultiplier *= m.ScoreMultiplier;

            // Scale score up, so it's comparable to other keymods
            scaledScore *= 1.0 / scoreMultiplier;

            // Arbitrary initial value for scaling pp in order to standardize distributions across game modes.
            // The specific number has no intrinsic meaning and can be adjusted as needed.
            double multiplier = 0.8;

            if (mods.Any(m => m is ModNoFail))
                multiplier *= 0.9;
            if (mods.Any(m => m is ModEasy))
                multiplier *= 0.5;

            double difficultyValue = computeDifficultyValue();
            double accValue = computeAccuracyValue(difficultyValue);
            double totalValue =
                Math.Pow(
                    Math.Pow(difficultyValue, 1.1) +
                    Math.Pow(accValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            return new ManiaPerformanceAttributes
            {
                Difficulty = difficultyValue,
                Accuracy = accValue,
                ScaledScore = scaledScore,
                Total = totalValue
            };
        }

        private double computeDifficultyValue()
        {
            double difficultyValue = Math.Pow(5 * Math.Max(1, Attributes.StarRating / 0.2) - 4.0, 2.2) / 135.0;

            difficultyValue *= 1.0 + 0.1 * Math.Min(1.0, totalHits / 1500.0);

            if (scaledScore <= 500000)
                difficultyValue = 0;
            else if (scaledScore <= 600000)
                difficultyValue *= (scaledScore - 500000) / 100000 * 0.3;
            else if (scaledScore <= 700000)
                difficultyValue *= 0.3 + (scaledScore - 600000) / 100000 * 0.25;
            else if (scaledScore <= 800000)
                difficultyValue *= 0.55 + (scaledScore - 700000) / 100000 * 0.20;
            else if (scaledScore <= 900000)
                difficultyValue *= 0.75 + (scaledScore - 800000) / 100000 * 0.15;
            else
                difficultyValue *= 0.90 + (scaledScore - 900000) / 100000 * 0.1;

            return difficultyValue;
        }

        private double computeAccuracyValue(double difficultyValue)
        {
            if (Attributes.GreatHitWindow <= 0)
                return 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Max(0.0, 0.2 - (Attributes.GreatHitWindow - 34) * 0.006667)
                                   * difficultyValue
                                   * Math.Pow(Math.Max(0.0, scaledScore - 960000) / 40000, 1.1);

            return accuracyValue;
        }

        private double totalHits => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
    }
}
