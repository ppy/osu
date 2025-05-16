// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuLegacyScoreMissCalculator
    {
        private readonly ScoreInfo score;
        private readonly OsuDifficultyAttributes attributes;

        public OsuLegacyScoreMissCalculator(ScoreInfo scoreInfo, OsuDifficultyAttributes attributes)
        {
            score = scoreInfo;
            this.attributes = attributes;
        }

        public double Calculate()
        {
            if (attributes.MaxCombo == 0 || score.LegacyTotalScore == null)
                return 0;

            double scoreV1Multiplier = attributes.LegacyScoreBaseMultiplier * LegacyScoreUtils.GetLegacyScoreMultiplier(score.Mods);
            double relevantComboPerObject = LegacyScoreUtils.CalculateRelevantScoreComboPerObject(attributes);

            double maximumMissCount = calculateMaximumComboBasedMissCount();

            double scoreObtainedDuringMaxCombo = LegacyScoreUtils.CalculateScoreAtCombo(score, attributes, score.MaxCombo, relevantComboPerObject, scoreV1Multiplier);
            double remainingScore = score.LegacyTotalScore.Value - scoreObtainedDuringMaxCombo;

            if (remainingScore <= 0)
                return maximumMissCount;

            double remainingCombo = attributes.MaxCombo - score.MaxCombo;
            double expectedRemainingScore = LegacyScoreUtils.CalculateScoreAtCombo(score, attributes, remainingCombo, relevantComboPerObject, scoreV1Multiplier);

            double scoreBasedMissCount = expectedRemainingScore / remainingScore;

            // If there's less then one miss detected - let combo-based miss count decide if this is FC or not
            scoreBasedMissCount = Math.Max(scoreBasedMissCount, 1);

            // Cap result by very harsh version of combo-based miss count
            return Math.Min(scoreBasedMissCount, maximumMissCount);
        }

        /// <summary>
        /// This function is a harsher version of current combo-based miss count, used to provide reasonable value for cases where score-based miss count can't do this.
        /// </summary>
        private double calculateMaximumComboBasedMissCount()
        {
            int countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);

            if (attributes.SliderCount <= 0)
                return countMiss;

            int countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            int countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);

            int totalImperfectHits = countOk + countMeh + countMiss;

            double missCount = 0;

            // Consider that full combo is maximum combo minus dropped slider tails since they don't contribute to combo but also don't break it
            // In classic scores we can't know the amount of dropped sliders so we estimate to 10% of all sliders on the map
            double fullComboThreshold = attributes.MaxCombo - 0.1 * attributes.SliderCount;

            if (score.MaxCombo < fullComboThreshold)
                missCount = Math.Pow(fullComboThreshold / Math.Max(1.0, score.MaxCombo), 2.5);

            // In classic scores there can't be more misses than a sum of all non-perfect judgements
            missCount = Math.Min(missCount, totalImperfectHits);

            return missCount;
        }
    }
}
