// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
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

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            mods = Score.Mods;
            scaledScore = Score.TotalScore;
            countPerfect = Score.Statistics.GetOrDefault(HitResult.Perfect);
            countGreat = Score.Statistics.GetOrDefault(HitResult.Great);
            countGood = Score.Statistics.GetOrDefault(HitResult.Good);
            countOk = Score.Statistics.GetOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetOrDefault(HitResult.Miss);

            if (mods.Any(m => !m.Ranked))
                return 0;

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

            double strainValue = computeStrainValue();
            double accValue = computeAccuracyValue(strainValue);
            double totalValue =
                Math.Pow(
                    Math.Pow(strainValue, 1.1) +
                    Math.Pow(accValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            if (categoryDifficulty != null)
            {
                categoryDifficulty["Strain"] = strainValue;
                categoryDifficulty["Accuracy"] = accValue;
            }

            return totalValue;
        }

        private double computeStrainValue()
        {
            // Obtain strain difficulty
            double strainValue = Math.Pow(5 * Math.Max(1, Attributes.StarRating / 0.2) - 4.0, 2.2) / 135.0;

            // Longer maps are worth more
            strainValue *= 1.0 + 0.1 * Math.Min(1.0, totalHits / 1500.0);

            if (scaledScore <= 500000)
                strainValue = 0;
            else if (scaledScore <= 600000)
                strainValue *= (scaledScore - 500000) / 100000 * 0.3;
            else if (scaledScore <= 700000)
                strainValue *= 0.3 + (scaledScore - 600000) / 100000 * 0.25;
            else if (scaledScore <= 800000)
                strainValue *= 0.55 + (scaledScore - 700000) / 100000 * 0.20;
            else if (scaledScore <= 900000)
                strainValue *= 0.75 + (scaledScore - 800000) / 100000 * 0.15;
            else
                strainValue *= 0.90 + (scaledScore - 900000) / 100000 * 0.1;

            return strainValue;
        }

        private double computeAccuracyValue(double strainValue)
        {
            if (Attributes.GreatHitWindow <= 0)
                return 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Max(0.0, 0.2 - (Attributes.GreatHitWindow - 34) * 0.006667)
                                   * strainValue
                                   * Math.Pow(Math.Max(0.0, scaledScore - 960000) / 40000, 1.1);

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            // accuracyValue *= Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));

            return accuracyValue;
        }

        private double totalHits => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
    }
}
