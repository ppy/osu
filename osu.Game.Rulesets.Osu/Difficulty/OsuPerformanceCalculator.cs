// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using MathNet.Numerics.Interpolation;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public new OsuDifficultyAttributes Attributes => (OsuDifficultyAttributes)base.Attributes;

        private Mod[] mods;

        private double accuracy;
        private int scoreMaxCombo;
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private double comboBasedMissCount;

        private const double combo_weight = 0.5;

        public OsuPerformanceCalculator(Ruleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            countGreat = Score.Statistics.GetOrDefault(HitResult.Great);
            countOk = Score.Statistics.GetOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetOrDefault(HitResult.Miss);
            comboBasedMissCount = calculateComboBasedMissCount();

            double multiplier = 1.12; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            // Custom multipliers for NoFail and SpunOut.
            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= Math.Max(0.90, 1.0 - 0.02 * countMiss);

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 1.0 - Math.Pow((double)Attributes.SpinnerCount / totalHits, 0.85);

            double aimValue = computeAimValue(categoryRatings);
            double speedValue = computeSpeedValue(categoryRatings);
            double accuracyValue = computeAccuracyValue();
            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1) +
                    Math.Pow(speedValue, 1.1) +
                    Math.Pow(accuracyValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", Attributes.MaxCombo);
            }

            return totalValue;
        }

        /// <summary>
        /// Calculates achieved difficulty for current score by taking difficulty sections of the map and
        /// interpolating between them using achieved combo percentage.
        /// </summary>
        private double achievedComboDifficulty(double[] mapComboDifficultySections)
        {
            double achievedComboPercentage = scoreMaxCombo / (double)Attributes.MaxCombo;
            return LinearSpline.InterpolateSorted(ProbabilityBasedSkill.COMBO_PERCENTAGES, mapComboDifficultySections)
                               .Interpolate(achievedComboPercentage);
        }

        /// <summary>
        /// Calculated achieved difficulty for current score by taking difficulty value of the map and
        /// multiplying it by expected miss count
        /// </summary>
        private double missCountDifficulty(double[] mapMissCountSections, double mapDifficulty)
        {
            if (countMiss == 0)
                return mapDifficulty;

            return mapDifficulty * LinearSpline.InterpolateSorted(mapMissCountSections, ProbabilityBasedSkill.MISS_STAR_RATING_MULTIPLIERS)
                                               .Interpolate(countMiss);
        }

        private double computeAimValue(Dictionary<string, double> categoryRatings = null)
        {
            double aimComboDifficulty = achievedComboDifficulty(Attributes.AimComboBasedDifficulties);
            double aimMissCountDifficulty = missCountDifficulty(Attributes.AimMissCounts, Attributes.AimComboBasedDifficulties.Last());

            double rawAim = Math.Pow(aimComboDifficulty, combo_weight) * Math.Pow(aimMissCountDifficulty, 1 - combo_weight);

            if (mods.Any(m => m is OsuModTouchDevice))
                rawAim = Math.Pow(rawAim, 0.8);

            double aimValue = Math.Pow(5.0f * Math.Max(1.0f, rawAim / 0.0675f) - 4.0f, 3.0f) / 100000.0f;

            // Penalize misses by assessing # of misses relative to the total # of objects.
            if (comboBasedMissCount > 0)
                aimValue *= Math.Pow(1 - Math.Pow(comboBasedMissCount / totalHits, 0.775), comboBasedMissCount);

            double approachRateFactor = 0.0;
            if (Attributes.ApproachRate > 10.33)
                approachRateFactor += 0.4 * (Attributes.ApproachRate - 10.33);
            else if (Attributes.ApproachRate < 8.0)
                approachRateFactor += 0.01 * (8.0 - Attributes.ApproachRate);

            // We scale with length since high AR is sensitive to memorization techniques.
            aimValue *= 1.0 + Math.Min(approachRateFactor, approachRateFactor * (totalHits / 1000.0));

            // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
            if (mods.Any(h => h is OsuModHidden))
                aimValue *= 1.0 + 0.04 * (12.0 - Attributes.ApproachRate);

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply object-based bonus for flashlight.
                aimValue *= 1.0 + 0.35 * Math.Min(1.0, totalHits / 200.0) +
                            (totalHits > 200
                                ? 0.3 * Math.Min(1.0, (totalHits - 200) / 300.0) +
                                  (totalHits > 500 ? (totalHits - 500) / 1200.0 : 0.0)
                                : 0.0);
            }

            // Scale the aim value with accuracy _slightly_
            aimValue *= 0.5 + accuracy / 2.0;
            // It is important to also consider accuracy difficulty when doing that
            aimValue *= 0.98 + Math.Pow(Attributes.OverallDifficulty, 2) / 2500;

            // Scale by % FC'd to encourage FC.
            aimValue *= 0.8 + 0.2 * (Score.MaxCombo / (double)Attributes.MaxCombo);

            if (categoryRatings != null)
            {
                categoryRatings.Add("Final Aim Difficulty", aimValue);
                categoryRatings.Add("Achieved Combo Aim Difficulty", aimComboDifficulty);
                categoryRatings.Add("Misscount Reduced Aim Difficulty", aimMissCountDifficulty);
            }

            return aimValue;
        }

        private double computeSpeedValue(Dictionary<string, double> categoryRatings = null)
        {
            double speedComboDifficulty = achievedComboDifficulty(Attributes.SpeedComboBasedDifficulties);
            double speedMissCountDifficulty = missCountDifficulty(Attributes.SpeedMissCounts, Attributes.SpeedComboBasedDifficulties.Last());

            double rawSpeed = Math.Pow(speedComboDifficulty, combo_weight) * Math.Pow(speedMissCountDifficulty, 1 - combo_weight);

            double speedValue = Math.Pow(5.0f * Math.Max(1.0f, rawSpeed / 0.0675f) - 4.0f, 3.0f) / 100000.0f;

            // Penalize misses by assessing # of misses relative to the total # of objects.
            if (comboBasedMissCount > 0)
                speedValue *= Math.Pow(1 - Math.Pow(comboBasedMissCount / totalHits, 0.775), comboBasedMissCount);

            double approachRateFactor = 0.0;
            if (Attributes.ApproachRate > 10.33)
                approachRateFactor += 0.4 * (Attributes.ApproachRate - 10.33);
            
            speedValue *= 1.0 + Math.Min(approachRateFactor, approachRateFactor * (totalHits / 1000.0));

            if (mods.Any(m => m is OsuModHidden))
                speedValue *= 1.0 + 0.04 * (12.0 - Attributes.ApproachRate);

            // Scale the speed value with accuracy and OD
            speedValue *= (0.95 + Math.Pow(Attributes.OverallDifficulty, 2) / 750) * Math.Pow(accuracy, (14.5 - Math.Max(Attributes.OverallDifficulty, 8)) / 2);
            // Scale the speed value with # of 50s to punish doubletapping.
            speedValue *= Math.Pow(0.98, countMeh < totalHits / 500.0 ? countMeh : countMeh - totalHits / 500.0);

            // Scale by % FC'd to encourage FC.
            speedValue *= 0.9 + 0.1 * (Score.MaxCombo / (double)Attributes.MaxCombo);

            if (categoryRatings != null)
            {
                categoryRatings.Add("Final Speed Difficulty", speedValue);
                categoryRatings.Add("Achieved Combo Speed Difficulty", speedComboDifficulty);
                categoryRatings.Add("Misscount Reduced Speed Difficulty", speedMissCountDifficulty);
            }

            return speedValue;
        }

        private double computeAccuracyValue()
        {
            // This percentage only considers HitCircles of any value - in this part of the calculation we focus on hitting the timing hit window
            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = Attributes.HitCircleCount;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((countGreat - (totalHits - amountHitObjectsWithAccuracy)) * 6 + countOk * 2 + countMeh) / (double)(amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points
            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Pow(1.52163, Attributes.OverallDifficulty) * Math.Pow(betterAccuracyPercentage, 24) * 2.83;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accuracyValue *= Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 1000.0, 0.3));

            if (mods.Any(m => m is OsuModHidden))
                accuracyValue *= 1.08;
            if (mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02;

            return accuracyValue;
        }

        private double calculateComboBasedMissCount()
        {
            // approximate the minimum number of misses and/or slider breaks from combo
            int beatmapMaxCombo = Attributes.MaxCombo;
            int countSliders = Attributes.HitSliderCount;
            double comboBasedMissCount;

            if (countSliders == 0)
            {
                if (scoreMaxCombo < beatmapMaxCombo)
                    comboBasedMissCount = (double)beatmapMaxCombo / scoreMaxCombo;
                else
                    comboBasedMissCount = 0;
            }
            else
            {
                double fullComboThreshold = beatmapMaxCombo - 0.1 * countSliders;
                if (scoreMaxCombo < fullComboThreshold)
                    comboBasedMissCount = fullComboThreshold / scoreMaxCombo;
                else
                    comboBasedMissCount = Math.Pow((beatmapMaxCombo - scoreMaxCombo) / (0.1 * countSliders), 3);
            }

            return Math.Max(countMiss, (int)comboBasedMissCount);
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;
        private int totalSuccessfulHits => countGreat + countOk + countMeh;
    }
}
