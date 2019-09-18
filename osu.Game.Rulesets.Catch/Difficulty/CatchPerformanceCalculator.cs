// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osuTK;

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

        public CatchPerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, ScoreInfo score)
            : base(ruleset, beatmap, score)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            mods = Score.Mods;

            var legacyScore = Score as LegacyScoreInfo;

            fruitsHit = legacyScore?.Count300 ?? Score.Statistics[HitResult.Perfect];
            ticksHit = legacyScore?.Count100 ?? 0;
            tinyTicksHit = legacyScore?.Count50 ?? 0;
            tinyTicksMissed = legacyScore?.CountKatu ?? 0;
            misses = Score.Statistics[HitResult.Miss];

            // Don't count scores made with supposedly unranked mods
            if (mods.Any(m => !m.Ranked))
                return 0;

            // We are heavily relying on aim in catch the beat
            double value = Math.Pow(5.0f * Math.Max(1.0f, Attributes.StarRating / 0.0049f) - 4.0f, 2.0f) / 100000.0f;

            // Longer maps are worth more. "Longer" means how many hits there are which can contribute to combo
            int numTotalHits = totalComboHits();

            // Longer maps are worth more
            float lengthBonus =
                0.95f + 0.4f * Math.Min(1.0f, numTotalHits / 3000.0f) +
                (numTotalHits > 3000 ? (float)Math.Log10(numTotalHits / 3000.0f) * 0.5f : 0.0f);

            // Longer maps are worth more
            value *= lengthBonus;

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            value *= Math.Pow(0.97f, misses);

            // Combo scaling
            float beatmapMaxCombo = Attributes.MaxCombo;
            if (beatmapMaxCombo > 0)
                value *= Math.Min(Math.Pow(Attributes.MaxCombo, 0.8f) / Math.Pow(beatmapMaxCombo, 0.8f), 1.0f);

            float approachRate = (float)Attributes.ApproachRate;
            float approachRateFactor = 1.0f;
            if (approachRate > 9.0f)
                approachRateFactor += 0.1f * (approachRate - 9.0f); // 10% for each AR above 9
            else if (approachRate < 8.0f)
                approachRateFactor += 0.025f * (8.0f - approachRate); // 2.5% for each AR below 8

            value *= approachRateFactor;

            if (mods.Any(m => m is ModHidden))
                // Hiddens gives nothing on max approach rate, and more the lower it is
                value *= 1.05f + 0.075f * (10.0f - Math.Min(10.0f, approachRate)); // 7.5% for each AR below 10

            if (mods.Any(m => m is ModFlashlight))
                // Apply length bonus again if flashlight is on simply because it becomes a lot harder on longer maps.
                value *= 1.35f * lengthBonus;

            // Scale the aim value with accuracy _slightly_
            value *= Math.Pow(accuracy(), 5.5f);

            // Custom multipliers for NoFail. SpunOut is not applicable.
            if (mods.Any(m => m is ModNoFail))
                value *= 0.90f;

            return value;
        }

        private float accuracy() => totalHits() == 0 ? 0 : MathHelper.Clamp((float)totalSuccessfulHits() / totalHits(), 0f, 1f);
        private int totalHits() => tinyTicksHit + ticksHit + fruitsHit + misses + tinyTicksMissed;
        private int totalSuccessfulHits() => tinyTicksHit + ticksHit + fruitsHit;
        private int totalComboHits() => misses + ticksHit + fruitsHit;
    }
}
