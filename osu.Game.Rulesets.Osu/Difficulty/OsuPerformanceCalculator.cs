// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public const double PERFORMANCE_BASE_MULTIPLIER = 1.15; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things.

        private bool usingClassicSliderAccuracy;

        private double accuracy;
        private int scoreMaxCombo;
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;

        /// <summary>
        /// Missed slider ticks that includes missed reverse arrows. Will only be correct on non-classic scores
        /// </summary>
        private int countSliderTickMiss;

        /// <summary>
        /// Amount of missed slider tails that don't break combo. Will only be correct on non-classic scores
        /// </summary>
        private int countSliderEndsDropped;

        /// <summary>
        /// Estimated total amount of combo breaks
        /// </summary>
        private double effectiveMissCount;

        private double clockRate;
        private double greatHitWindow;
        private double okHitWindow;
        private double mehHitWindow;
        private double overallDifficulty;
        private double approachRate;

        private double? speedDeviation;

        public OsuPerformanceCalculator()
            : base(new OsuRuleset())
        {
        }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var osuAttributes = (OsuDifficultyAttributes)attributes;

            usingClassicSliderAccuracy = score.Mods.OfType<OsuModClassic>().Any(m => m.NoSliderHeadAccuracy.Value);

            accuracy = score.Accuracy;
            scoreMaxCombo = score.MaxCombo;
            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);
            countSliderEndsDropped = osuAttributes.SliderCount - score.Statistics.GetValueOrDefault(HitResult.SliderTailHit);
            countSliderTickMiss = score.Statistics.GetValueOrDefault(HitResult.LargeTickMiss);
            effectiveMissCount = countMiss;

            var difficulty = score.BeatmapInfo!.Difficulty.Clone();

            score.Mods.OfType<IApplicableToDifficulty>().ForEach(m => m.ApplyToDifficulty(difficulty));

            var track = new TrackVirtual(10000);
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            clockRate = track.Rate;

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(difficulty.OverallDifficulty);

            greatHitWindow = hitWindows.WindowFor(HitResult.Great) / clockRate;
            okHitWindow = hitWindows.WindowFor(HitResult.Ok) / clockRate;
            mehHitWindow = hitWindows.WindowFor(HitResult.Meh) / clockRate;

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            overallDifficulty = (80 - greatHitWindow) / 6;
            approachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5;

            if (osuAttributes.SliderCount > 0)
            {
                if (usingClassicSliderAccuracy)
                {
                    // Consider that full combo is maximum combo minus dropped slider tails since they don't contribute to combo but also don't break it
                    // In classic scores we can't know the amount of dropped sliders so we estimate to 10% of all sliders on the map
                    double fullComboThreshold = attributes.MaxCombo - 0.1 * osuAttributes.SliderCount;

                    if (scoreMaxCombo < fullComboThreshold)
                        effectiveMissCount = fullComboThreshold / Math.Max(1.0, scoreMaxCombo);

                    // In classic scores there can't be more misses than a sum of all non-perfect judgements
                    effectiveMissCount = Math.Min(effectiveMissCount, totalImperfectHits);
                }
                else
                {
                    double fullComboThreshold = attributes.MaxCombo - countSliderEndsDropped;

                    if (scoreMaxCombo < fullComboThreshold)
                        effectiveMissCount = fullComboThreshold / Math.Max(1.0, scoreMaxCombo);

                    // Combine regular misses with tick misses since tick misses break combo as well
                    effectiveMissCount = Math.Min(effectiveMissCount, countSliderTickMiss + countMiss);
                }
            }

            effectiveMissCount = Math.Max(countMiss, effectiveMissCount);
            effectiveMissCount = Math.Min(totalHits, effectiveMissCount);

            double multiplier = PERFORMANCE_BASE_MULTIPLIER;

            if (score.Mods.Any(m => m is OsuModNoFail))
                multiplier *= Math.Max(0.90, 1.0 - 0.02 * effectiveMissCount);

            if (score.Mods.Any(m => m is OsuModSpunOut) && totalHits > 0)
                multiplier *= 1.0 - Math.Pow((double)osuAttributes.SpinnerCount / totalHits, 0.85);

            if (score.Mods.Any(h => h is OsuModRelax))
            {
                // https://www.desmos.com/calculator/bc9eybdthb
                // we use OD13.3 as maximum since it's the value at which great hitwidow becomes 0
                // this is well beyond currently maximum achievable OD which is 12.17 (DTx2 + DA with OD11)
                double okMultiplier = Math.Max(0.0, overallDifficulty > 0.0 ? 1 - Math.Pow(overallDifficulty / 13.33, 1.8) : 1.0);
                double mehMultiplier = Math.Max(0.0, overallDifficulty > 0.0 ? 1 - Math.Pow(overallDifficulty / 13.33, 5) : 1.0);

                // As we're adding Oks and Mehs to an approximated number of combo breaks the result can be higher than total hits in specific scenarios (which breaks some calculations) so we need to clamp it.
                effectiveMissCount = Math.Min(effectiveMissCount + countOk * okMultiplier + countMeh * mehMultiplier, totalHits);
            }

            speedDeviation = calculateSpeedDeviation(osuAttributes);

            double aimValue = computeAimValue(score, osuAttributes);
            double speedValue = computeSpeedValue(score, osuAttributes);
            double accuracyValue = computeAccuracyValue(score, osuAttributes);
            double flashlightValue = computeFlashlightValue(score, osuAttributes);

            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1) +
                    Math.Pow(speedValue, 1.1) +
                    Math.Pow(accuracyValue, 1.1) +
                    Math.Pow(flashlightValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            return new OsuPerformanceAttributes
            {
                Aim = aimValue,
                Speed = speedValue,
                Accuracy = accuracyValue,
                Flashlight = flashlightValue,
                EffectiveMissCount = effectiveMissCount,
                SpeedDeviation = speedDeviation,
                Total = totalValue
            };
        }

        private double computeAimValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (score.Mods.Any(h => h is OsuModAutopilot))
                return 0.0;

            double aimDifficulty = attributes.AimDifficulty;

            if (attributes.SliderCount > 0 && attributes.AimDifficultSliderCount > 0)
            {
                double estimateImproperlyFollowedDifficultSliders;

                if (usingClassicSliderAccuracy)
                {
                    // When the score is considered classic (regardless if it was made on old client or not) we consider all missing combo to be dropped difficult sliders
                    int maximumPossibleDroppedSliders = totalImperfectHits;
                    estimateImproperlyFollowedDifficultSliders = Math.Clamp(Math.Min(maximumPossibleDroppedSliders, attributes.MaxCombo - scoreMaxCombo), 0, attributes.AimDifficultSliderCount);
                }
                else
                {
                    // We add tick misses here since they too mean that the player didn't follow the slider properly
                    // We however aren't adding misses here because missing slider heads has a harsh penalty by itself and doesn't mean that the rest of the slider wasn't followed properly
                    estimateImproperlyFollowedDifficultSliders = Math.Clamp(countSliderEndsDropped + countSliderTickMiss, 0, attributes.AimDifficultSliderCount);
                }

                double sliderNerfFactor = (1 - attributes.SliderFactor) * Math.Pow(1 - estimateImproperlyFollowedDifficultSliders / attributes.AimDifficultSliderCount, 3) + attributes.SliderFactor;
                aimDifficulty *= sliderNerfFactor;
            }

            double aimValue = OsuStrainSkill.DifficultyToPerformance(aimDifficulty);

            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            aimValue *= lengthBonus;

            if (effectiveMissCount > 0)
                aimValue *= calculateMissPenalty(effectiveMissCount, attributes.AimDifficultStrainCount);

            double approachRateFactor = 0.0;
            if (approachRate > 10.33)
                approachRateFactor = 0.3 * (approachRate - 10.33);
            else if (approachRate < 8.0)
                approachRateFactor = 0.05 * (8.0 - approachRate);

            if (score.Mods.Any(h => h is OsuModRelax))
                approachRateFactor = 0.0;

            aimValue *= 1.0 + approachRateFactor * lengthBonus; // Buff for longer maps with high AR.

            if (score.Mods.Any(m => m is OsuModBlinds))
                aimValue *= 1.3 + (totalHits * (0.0016 / (1 + 2 * effectiveMissCount)) * Math.Pow(accuracy, 16)) * (1 - 0.003 * attributes.DrainRate * attributes.DrainRate);
            else if (score.Mods.Any(m => m is OsuModHidden || m is OsuModTraceable))
            {
                // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
                aimValue *= 1.0 + 0.04 * (12.0 - approachRate);
            }

            aimValue *= accuracy;
            // It is important to consider accuracy difficulty when scaling with accuracy.
            aimValue *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return aimValue;
        }

        private double computeSpeedValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (score.Mods.Any(h => h is OsuModRelax) || speedDeviation == null)
                return 0.0;

            double speedValue = OsuStrainSkill.DifficultyToPerformance(attributes.SpeedDifficulty);

            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            speedValue *= lengthBonus;

            if (effectiveMissCount > 0)
                speedValue *= calculateMissPenalty(effectiveMissCount, attributes.SpeedDifficultStrainCount);

            double approachRateFactor = 0.0;
            if (approachRate > 10.33)
                approachRateFactor = 0.3 * (approachRate - 10.33);

            if (score.Mods.Any(h => h is OsuModAutopilot))
                approachRateFactor = 0.0;

            speedValue *= 1.0 + approachRateFactor * lengthBonus; // Buff for longer maps with high AR.

            if (score.Mods.Any(m => m is OsuModBlinds))
            {
                // Increasing the speed value by object count for Blinds isn't ideal, so the minimum buff is given.
                speedValue *= 1.12;
            }
            else if (score.Mods.Any(m => m is OsuModHidden || m is OsuModTraceable))
            {
                // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
                speedValue *= 1.0 + 0.04 * (12.0 - approachRate);
            }

            double speedHighDeviationMultiplier = calculateSpeedHighDeviationNerf(attributes);
            speedValue *= speedHighDeviationMultiplier;

            // Calculate accuracy assuming the worst case scenario
            double relevantTotalDiff = Math.Max(0, totalHits - attributes.SpeedNoteCount);
            double relevantCountGreat = Math.Max(0, countGreat - relevantTotalDiff);
            double relevantCountOk = Math.Max(0, countOk - Math.Max(0, relevantTotalDiff - countGreat));
            double relevantCountMeh = Math.Max(0, countMeh - Math.Max(0, relevantTotalDiff - countGreat - countOk));
            double relevantAccuracy = attributes.SpeedNoteCount == 0 ? 0 : (relevantCountGreat * 6.0 + relevantCountOk * 2.0 + relevantCountMeh) / (attributes.SpeedNoteCount * 6.0);

            // Scale the speed value with accuracy and OD.
            speedValue *= (0.95 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 750) * Math.Pow((accuracy + relevantAccuracy) / 2.0, (14.5 - overallDifficulty) / 2);

            return speedValue;
        }

        private double computeAccuracyValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (score.Mods.Any(h => h is OsuModRelax))
                return 0.0;

            // This percentage only considers HitCircles of any value - in this part of the calculation we focus on hitting the timing hit window.
            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = attributes.HitCircleCount;
            if (!usingClassicSliderAccuracy)
                amountHitObjectsWithAccuracy += attributes.SliderCount;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((countGreat - Math.Max(totalHits - amountHitObjectsWithAccuracy, 0)) * 6 + countOk * 2 + countMeh) / (double)(amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points.
            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution.
            double accuracyValue = Math.Pow(1.52163, overallDifficulty) * Math.Pow(betterAccuracyPercentage, 24) * 2.83;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer.
            accuracyValue *= Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 1000.0, 0.3));

            // Increasing the accuracy value by object count for Blinds isn't ideal, so the minimum buff is given.
            if (score.Mods.Any(m => m is OsuModBlinds))
                accuracyValue *= 1.14;
            else if (score.Mods.Any(m => m is OsuModHidden || m is OsuModTraceable))
                accuracyValue *= 1.08;

            if (score.Mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02;

            return accuracyValue;
        }

        private double computeFlashlightValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (!score.Mods.Any(h => h is OsuModFlashlight))
                return 0.0;

            double flashlightValue = Flashlight.DifficultyToPerformance(attributes.FlashlightDifficulty);

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            if (effectiveMissCount > 0)
                flashlightValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), Math.Pow(effectiveMissCount, .875));

            flashlightValue *= getComboScalingFactor(attributes);

            // Account for shorter maps having a higher ratio of 0 combo/100 combo flashlight radius.
            flashlightValue *= 0.7 + 0.1 * Math.Min(1.0, totalHits / 200.0) +
                               (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            // Scale the flashlight value with accuracy _slightly_.
            flashlightValue *= 0.5 + accuracy / 2.0;
            // It is important to also consider accuracy difficulty when doing that.
            flashlightValue *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return flashlightValue;
        }

        /// <summary>
        /// Estimates player's deviation on speed notes using <see cref="calculateDeviation"/>, assuming worst-case.
        /// Treats all speed notes as hit circles.
        /// </summary>
        private double? calculateSpeedDeviation(OsuDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return null;

            // Calculate accuracy assuming the worst case scenario
            double speedNoteCount = attributes.SpeedNoteCount;
            speedNoteCount += (totalHits - attributes.SpeedNoteCount) * 0.1;

            // Assume worst case: all mistakes were on speed notes
            double relevantCountMiss = Math.Min(countMiss, speedNoteCount);
            double relevantCountMeh = Math.Min(countMeh, speedNoteCount - relevantCountMiss);
            double relevantCountOk = Math.Min(countOk, speedNoteCount - relevantCountMiss - relevantCountMeh);
            double relevantCountGreat = Math.Max(0, speedNoteCount - relevantCountMiss - relevantCountMeh - relevantCountOk);

            return calculateDeviation(attributes, relevantCountGreat, relevantCountOk, relevantCountMeh, relevantCountMiss);
        }

        /// <summary>
        /// Estimates the player's tap deviation based on the OD, given number of greats, oks, mehs and misses,
        /// assuming the player's mean hit error is 0. The estimation is consistent in that two SS scores on the same map with the same settings
        /// will always return the same deviation. Misses are ignored because they are usually due to misaiming.
        /// Greats and oks are assumed to follow a normal distribution, whereas mehs are assumed to follow a uniform distribution.
        /// </summary>
        private double? calculateDeviation(OsuDifficultyAttributes attributes, double relevantCountGreat, double relevantCountOk, double relevantCountMeh, double relevantCountMiss)
        {
            if (relevantCountGreat + relevantCountOk + relevantCountMeh <= 0)
                return null;

            double objectCount = relevantCountGreat + relevantCountOk + relevantCountMeh + relevantCountMiss;

            // The probability that a player hits a circle is unknown, but we can estimate it to be
            // the number of greats on circles divided by the number of circles, and then add one
            // to the number of circles as a bias correction.
            double n = Math.Max(1, objectCount - relevantCountMiss - relevantCountMeh);
            const double z = 2.32634787404; // 99% critical value for the normal distribution (one-tailed).

            // Proportion of greats hit on circles, ignoring misses and 50s.
            double p = relevantCountGreat / n;

            // We can be 99% confident that p is at least this value.
            double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

            // Compute the deviation assuming greats and oks are normally distributed, and mehs are uniformly distributed.
            // Begin with greats and oks first. Ignoring mehs, we can be 99% confident that the deviation is not higher than:
            double deviation = greatHitWindow / (Math.Sqrt(2) * DifficultyCalculationUtils.ErfInv(pLowerBound));

            double randomValue = Math.Sqrt(2 / Math.PI) * okHitWindow * Math.Exp(-0.5 * Math.Pow(okHitWindow / deviation, 2))
                                 / (deviation * DifficultyCalculationUtils.Erf(okHitWindow / (Math.Sqrt(2) * deviation)));

            deviation *= Math.Sqrt(1 - randomValue);

            // Value deviation approach as greatCount approaches 0
            double limitValue = okHitWindow / Math.Sqrt(3);

            // If precision is not enough to compute true deviation - use limit value
            if (pLowerBound == 0 || randomValue >= 1 || deviation > limitValue)
                deviation = limitValue;

            // Then compute the variance for mehs.
            double mehVariance = (mehHitWindow * mehHitWindow + okHitWindow * mehHitWindow + okHitWindow * okHitWindow) / 3;

            // Find the total deviation.
            deviation = Math.Sqrt(((relevantCountGreat + relevantCountOk) * Math.Pow(deviation, 2) + relevantCountMeh * mehVariance) / (relevantCountGreat + relevantCountOk + relevantCountMeh));

            return deviation;
        }

        // Calculates multiplier for speed to account for improper tapping based on the deviation and speed difficulty
        // https://www.desmos.com/calculator/dmogdhzofn
        private double calculateSpeedHighDeviationNerf(OsuDifficultyAttributes attributes)
        {
            if (speedDeviation == null)
                return 0;

            double speedValue = OsuStrainSkill.DifficultyToPerformance(attributes.SpeedDifficulty);

            // Decides a point where the PP value achieved compared to the speed deviation is assumed to be tapped improperly. Any PP above this point is considered "excess" speed difficulty.
            // This is used to cause PP above the cutoff to scale logarithmically towards the original speed value thus nerfing the value.
            double excessSpeedDifficultyCutoff = 100 + 220 * Math.Pow(22 / speedDeviation.Value, 6.5);

            if (speedValue <= excessSpeedDifficultyCutoff)
                return 1.0;

            const double scale = 50;
            double adjustedSpeedValue = scale * (Math.Log((speedValue - excessSpeedDifficultyCutoff) / scale + 1) + excessSpeedDifficultyCutoff / scale);

            // 220 UR and less are considered tapped correctly to ensure that normal scores will be punished as little as possible
            double lerp = 1 - DifficultyCalculationUtils.ReverseLerp(speedDeviation.Value, 22.0, 27.0);
            adjustedSpeedValue = double.Lerp(adjustedSpeedValue, speedValue, lerp);

            return adjustedSpeedValue / speedValue;
        }

        // Miss penalty assumes that a player will miss on the hardest parts of a map,
        // so we use the amount of relatively difficult sections to adjust miss penalty
        // to make it more punishing on maps with lower amount of hard sections.
        private double calculateMissPenalty(double missCount, double difficultStrainCount) => 0.96 / ((missCount / (4 * Math.Pow(Math.Log(difficultStrainCount), 0.94))) + 1);
        private double getComboScalingFactor(OsuDifficultyAttributes attributes) => attributes.MaxCombo <= 0 ? 1.0 : Math.Min(Math.Pow(scoreMaxCombo, 0.8) / Math.Pow(attributes.MaxCombo, 0.8), 1.0);

        private int totalHits => countGreat + countOk + countMeh + countMiss;
        private int totalSuccessfulHits => countGreat + countOk + countMeh;
        private int totalImperfectHits => countOk + countMeh + countMiss;
    }
}
