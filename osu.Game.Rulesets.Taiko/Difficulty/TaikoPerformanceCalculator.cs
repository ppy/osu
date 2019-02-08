// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
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
        private int countGood;
        private int countMeh;
        private int countMiss;

        public TaikoPerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, ScoreInfo score)
            : base(ruleset, beatmap, score)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            mods = Score.Mods;
            countGreat = Convert.ToInt32(Score.Statistics[HitResult.Great]);
            countGood = Convert.ToInt32(Score.Statistics[HitResult.Good]);
            countMeh = Convert.ToInt32(Score.Statistics[HitResult.Meh]);
            countMiss = Convert.ToInt32(Score.Statistics[HitResult.Miss]);

            // Don't count scores made with supposedly unranked mods
            if (mods.Any(m => !m.Ranked))
                return 0;

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 1.1; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is ModNoFail))
                multiplier *= 0.90;

            if (mods.Any(m => m is ModHidden))
                multiplier *= 1.10;

            double strainValue = computeStrainValue();
            double accuracyValue = computeAccuracyValue();
            double totalValue =
                Math.Pow(
                    Math.Pow(strainValue, 1.1) +
                    Math.Pow(accuracyValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            if (categoryDifficulty != null)
            {
                categoryDifficulty["Strain"] = strainValue;
                categoryDifficulty["Accuracy"] = accuracyValue;
            }

            return totalValue;
        }

        private double computeStrainValue()
        {
            double strainValue = Math.Pow(5.0 * Math.Max(1.0, Attributes.StarRating / 0.0075) - 4.0, 2.0) / 100000.0;

            // Longer maps are worth more
            double lengthBonus = 1 + 0.1f * Math.Min(1.0, totalHits / 1500.0);
            strainValue *= lengthBonus;

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            strainValue *= Math.Pow(0.985, countMiss);

            // Combo scaling
            if (Attributes.MaxCombo > 0)
                strainValue *= Math.Min(Math.Pow(Score.MaxCombo, 0.5) / Math.Pow(Attributes.MaxCombo, 0.5), 1.0);

            if (mods.Any(m => m is ModHidden))
                strainValue *= 1.025;

            if (mods.Any(m => m is ModFlashlight<TaikoHitObject>))
                // Apply length bonus again if flashlight is on simply because it becomes a lot harder on longer maps.
                strainValue *= 1.05 * lengthBonus;

            // Scale the speed value with accuracy _slightly_
            return strainValue * Score.Accuracy;
        }

        private double computeAccuracyValue()
        {
            if (Attributes.GreatHitWindow <= 0)
                return 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accValue = Math.Pow(150.0 / Attributes.GreatHitWindow, 1.1) * Math.Pow(Score.Accuracy, 15) * 22.0;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            return accValue * Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));
        }

        private int totalHits => countGreat + countGood + countMeh + countMiss;
    }
}
