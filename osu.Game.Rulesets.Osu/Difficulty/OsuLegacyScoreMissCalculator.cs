// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuLegacyScoreMissCalculator
    {
        private readonly ScoreInfo score;
        private readonly OsuDifficultyAttributes attributes;

        private bool scoreV2;

        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;

        public OsuLegacyScoreMissCalculator(ScoreInfo scoreInfo, OsuDifficultyAttributes attributes)
        {
            score = scoreInfo;
            this.attributes = attributes;

            scoreV2 = score.Mods.Any(m => m is ModScoreV2);
            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);
        }

        public double Calculate()
        {
            if (attributes.MaxCombo == 0 || score.LegacyTotalScore == null)
                return 0;

            // Start from mod multiplier
            double scoreMultiplier = getLegacyModMultiplier();

            // We need to get only combo portion of ScoreV2, since this is rescaled ScoreV1 we need to compute misscount
            double scoreLegacyTotalScore = score.LegacyTotalScore.Value - (scoreV2 ? 300000 * Math.Pow(score.Accuracy, 10) * scoreMultiplier : 0);

            // Add base multiplier. We do it for ScoreV2 as well, since we would scale by maximum score anyway
            scoreMultiplier *= attributes.LegacyScoreBaseMultiplier;

            double relevantComboPerObject = calculateRelevantScoreComboPerObject();
            double maximumMissCount = calculateMaximumComboBasedMissCount();

            double scoreObtainedDuringMaxCombo = calculateScoreAtCombo(score.MaxCombo, relevantComboPerObject, scoreMultiplier);

            // If ScoreV2 - we scale it by maximum score. Don't multiply max score by mod multiplier, since we need to keep mod multiplier in the score
            if (scoreV2) scoreObtainedDuringMaxCombo *= 700000 / attributes.MaximumLegacyComboScore;

            double remainingScore = scoreLegacyTotalScore - scoreObtainedDuringMaxCombo;

            if (remainingScore <= 0)
                return maximumMissCount;

            double remainingCombo = attributes.MaxCombo - score.MaxCombo;
            double expectedRemainingScore = calculateScoreAtCombo(remainingCombo, relevantComboPerObject, scoreMultiplier);

            if (scoreV2) expectedRemainingScore *= 700000 / attributes.MaximumLegacyComboScore;

            double scoreBasedMissCount = expectedRemainingScore / remainingScore;

            // If there's less then one miss detected - let combo-based miss count decide if this is FC or not
            scoreBasedMissCount = Math.Max(scoreBasedMissCount, 1);

            // Cap result by very harsh version of combo-based miss count
            return Math.Min(scoreBasedMissCount, maximumMissCount);
        }

        /// <summary>
        /// Calculates the amount of score that would be achieved at a given combo.
        /// </summary>
        private double calculateScoreAtCombo(double combo, double relevantComboPerObject, double scoreV1Multiplier)
        {
            double estimatedObjects = combo / relevantComboPerObject - 1;

            // The combo portion of ScoreV1 follows arithmetic progression
            // Therefore, we calculate the combo portion of score using the combo per object and our current combo.
            double comboScore = relevantComboPerObject > 0 ? (2 * (relevantComboPerObject - 1) + (estimatedObjects - 1) * relevantComboPerObject) * estimatedObjects / 2 : 0;

            // We then apply the accuracy and ScoreV1 multipliers to the resulting score.
            comboScore *= 300 / 25 * scoreV1Multiplier;

            // For scoreV2 we need only combo score not scaled by accuracy.
            // This is technically incorrect because scoreV2 is using different formula,
            // but we have to sacrifice estimation precision, since it's not as important here.
            if (scoreV2) return comboScore;

            double objectsHit = (totalHits - countMiss) * combo / attributes.MaxCombo;

            // Score also has a non-combo portion we need to create the final score value.
            double nonComboScore = (300 + attributes.NestedScorePerObject) * objectsHit;

            return (comboScore + nonComboScore) * score.Accuracy;
        }

        /// <summary>
        /// Calculates the relevant combo per object for legacy score.
        /// This assumes a uniform distribution for circles and sliders.
        /// This handles cases where objects (such as buzz sliders) do not fit a normal arithmetic progression model.
        /// </summary>
        private double calculateRelevantScoreComboPerObject()
        {
            double comboScore = attributes.MaximumLegacyComboScore;

            // We then reverse apply the ScoreV1 multipliers to get the raw value.
            comboScore /= 300.0 / 25.0 * attributes.LegacyScoreBaseMultiplier;

            // Reverse the arithmetic progression to work out the amount of combo per object based on the score.
            double result = (attributes.MaxCombo - 2) * attributes.MaxCombo;
            result /= Math.Max(attributes.MaxCombo + 2 * (comboScore - 1), 1);

            return result;
        }

        /// <summary>
        /// This function is a harsher version of current combo-based miss count, used to provide reasonable value for cases where score-based miss count can't do this.
        /// </summary>
        private double calculateMaximumComboBasedMissCount()
        {
            if (attributes.SliderCount <= 0)
                return countMiss;

            int totalImperfectHits = countOk + countMeh + countMiss;

            double missCount = 0;

            // Consider that full combo is maximum combo minus dropped slider tails since they don't contribute to combo but also don't break it
            // In classic scores we can't know the amount of dropped sliders so we estimate to 10% of all sliders on the map
            double fullComboThreshold = attributes.MaxCombo - 0.1 * attributes.SliderCount;

            if (score.MaxCombo < fullComboThreshold)
                missCount = Math.Pow(fullComboThreshold / Math.Max(1.0, score.MaxCombo), 2.5);

            // In classic scores there can't be more misses than a sum of all non-perfect judgements
            missCount = Math.Min(missCount, totalImperfectHits);

            // Every slider has *at least* 2 combo attributed in classic mechanics.
            // If they broke on a slider with a tick, then this still works since they would have lost at least 2 combo (the tick and the end)
            // Using this as a max means a score that loses 1 combo on a map can't possibly have been a slider break.
            // It must have been a slider end.
            int maxPossibleSliderBreaks = Math.Min(attributes.SliderCount, (attributes.MaxCombo - score.MaxCombo) / 2);

            int scoreMissCount = score.Statistics.GetValueOrDefault(HitResult.Miss);

            double sliderBreaks = missCount - scoreMissCount;

            if (sliderBreaks > maxPossibleSliderBreaks)
                missCount = scoreMissCount + maxPossibleSliderBreaks;

            return missCount;
        }

        /// <remarks>
        /// Logic copied from <see cref="OsuLegacyScoreSimulator.GetLegacyScoreMultiplier"/>.
        /// </remarks>
        private double getLegacyModMultiplier()
        {
            double multiplier = 1.0;

            foreach (var mod in score.Mods)
            {
                switch (mod)
                {
                    case OsuModNoFail:
                        multiplier *= scoreV2 ? 1.0 : 0.5;
                        break;

                    case OsuModEasy:
                        multiplier *= 0.5;
                        break;

                    case OsuModHalfTime:
                    case OsuModDaycore:
                        multiplier *= 0.3;
                        break;

                    case OsuModHidden:
                        multiplier *= 1.06;
                        break;

                    case OsuModHardRock:
                        multiplier *= scoreV2 ? 1.10 : 1.06;
                        break;

                    case OsuModDoubleTime:
                    case OsuModNightcore:
                        multiplier *= scoreV2 ? 1.20 : 1.12;
                        break;

                    case OsuModFlashlight:
                        multiplier *= 1.12;
                        break;

                    case OsuModSpunOut:
                        multiplier *= 0.9;
                        break;

                    case OsuModRelax:
                    case OsuModAutopilot:
                        return 0;
                }
            }

            return multiplier;
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;
    }
}
