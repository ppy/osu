// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchPerformanceCalculator : PerformanceCalculator
    {
        protected new CatchDifficultyAttributes Attributes => (CatchDifficultyAttributes)base.Attributes;

        private Mod[] mods;

        private int fruitsHit;
        private int ticksHit;
        private int tinyTicksHit;
        private int tinyTicksMissed;
        private int misses;

        public CatchPerformanceCalculator(Ruleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            mods = Score.Mods;

            fruitsHit = Score.Statistics.GetValueOrDefault(HitResult.Great);
            ticksHit = Score.Statistics.GetValueOrDefault(HitResult.LargeTickHit);
            tinyTicksHit = Score.Statistics.GetValueOrDefault(HitResult.SmallTickHit);
            tinyTicksMissed = Score.Statistics.GetValueOrDefault(HitResult.SmallTickMiss);
            misses = Score.Statistics.GetValueOrDefault(HitResult.Miss);

            // We are heavily relying on aim in catch the beat
            double value = Math.Pow(5.0 * Math.Max(1.0, Attributes.StarRating / 0.0049) - 4.0, 2.0) / 100000.0;

            // Longer maps are worth more. "Longer" means how many hits there are which can contribute to combo
            int numTotalHits = totalComboHits();

            // Longer maps are worth more
            double lenghtFactor = numTotalHits * 0.5 + Attributes.DirectionChangeCount * 0.9;
            double lengthBonus = 0.84 + 0.38 * (lenghtFactor / 1700);

            // Longer maps are worth more
            value *= lengthBonus;

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            value *= Math.Pow(0.96, misses);

            // Combo scaling
            if (Attributes.MaxCombo > 0)
                value *= Math.Min(Math.Pow(Score.MaxCombo, 0.5) / Math.Pow(Attributes.MaxCombo, 0.5), 1.0);

            double approachRate = Attributes.ApproachRate;
            double approachRateFactor = 1.0;
            if (approachRate > 9.0)
                approachRateFactor += 0.1 * (approachRate - 9.0); // 10% for each AR above 9
            if (approachRate > 10.0)
                approachRateFactor += 0.1 * (approachRate - 10.0); // Additional 10% at AR 11, 30% total
            else if (approachRate < 8.0)
                approachRateFactor += 0.04 * (8.0 - approachRate); // 4% for each AR below 8

            value *= approachRateFactor;

            if (mods.Any(m => m is ModHidden))
            {
                // Hiddens gives almost nothing on max approach rate, and more the lower it is
                if (approachRate <= 10.0)
                    value *= 1.05 + 0.10 * (10.0 - Math.Min(10.0, approachRate)); // 10% for each AR below 10
                else if (approachRate > 10.0)
                    value *= 1.01 + 0.04 * (11.0 - Math.Min(11.0, approachRate)); // 5% at AR 10, 1% at AR 11

                if (approachRate <= 9.0)
                    value *= 1 + 0.04 * (8.0 - approachRate); // Additional 4% for each AR below 9
            }

            if (mods.Any(m => m is ModFlashlight))
            {
                // Apply length bonus again if flashlight is on simply because it becomes a lot harder on longer maps.
                value *= 1.35 * lengthBonus;

                if (approachRate > 8.0f)
                    value *= 0.1f * (approachRate - 8.0) + 1; // 10% for each AR above 8

                if (approachRate < 8.0f)
                    value *= 0.06f * (8.0 - approachRate) + 1; // -6% for each AR below 8
            }

            // Scale the aim value with accuracy _slightly_
            value *= Math.Pow(accuracy(), 6);

            // Custom multiplier for HalfTime -> slower catcher = easier to control
            if (mods.Any(m => m is ModHalfTime))
                value *= 0.90;

            // Custom multipliers for NoFail. SpunOut is not applicable.
            if (mods.Any(m => m is ModNoFail))
                value *= 0.90;

            return value;
        }

        private double accuracy() => totalHits() == 0 ? 0 : Math.Clamp((double)totalSuccessfulHits() / totalHits(), 0, 1);
        private int totalHits() => tinyTicksHit + ticksHit + fruitsHit + misses + tinyTicksMissed;
        private int totalSuccessfulHits() => tinyTicksHit + ticksHit + fruitsHit;
        private int totalComboHits() => misses + ticksHit + fruitsHit;
    }
}
