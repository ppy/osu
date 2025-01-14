// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Utils;

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

        private double? deviation;
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
                double okMultiplier = Math.Max(0.0, osuAttributes.OverallDifficulty > 0.0 ? 1 - Math.Pow(osuAttributes.OverallDifficulty / 13.33, 1.8) : 1.0);
                double mehMultiplier = Math.Max(0.0, osuAttributes.OverallDifficulty > 0.0 ? 1 - Math.Pow(osuAttributes.OverallDifficulty / 13.33, 5) : 1.0);

                // As we're adding Oks and Mehs to an approximated number of combo breaks the result can be higher than total hits in specific scenarios (which breaks some calculations) so we need to clamp it.
                effectiveMissCount = Math.Min(effectiveMissCount + countOk * okMultiplier + countMeh * mehMultiplier, totalHits);
            }

            speedDeviation = calculateSpeedDeviation(osuAttributes);
            deviation = calculateDeviation(score, osuAttributes);

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
            if (score.Mods.Any(h => h is OsuModAutopilot) || deviation == null)
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
                    estimateImproperlyFollowedDifficultSliders = Math.Min(countSliderEndsDropped + countSliderTickMiss, attributes.AimDifficultSliderCount);
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
            if (attributes.ApproachRate > 10.33)
                approachRateFactor = 0.3 * (attributes.ApproachRate - 10.33);
            else if (attributes.ApproachRate < 8.0)
                approachRateFactor = 0.05 * (8.0 - attributes.ApproachRate);

            if (score.Mods.Any(h => h is OsuModRelax))
                approachRateFactor = 0.0;

            aimValue *= 1.0 + approachRateFactor * lengthBonus; // Buff for longer maps with high AR.

            if (score.Mods.Any(m => m is OsuModBlinds))
                aimValue *= 1.3 + (totalHits * (0.0016 / (1 + 2 * effectiveMissCount)) * Math.Pow(accuracy, 16)) * (1 - 0.003 * attributes.DrainRate * attributes.DrainRate);
            else if (score.Mods.Any(m => m is OsuModHidden || m is OsuModTraceable))
            {
                // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
                aimValue *= 1.0 + 0.04 * (12.0 - attributes.ApproachRate);
            }

            // OD 11.11 SS stays the same.
            aimValue *= 1 + 119 / 4050.0;

            // Scale the aim value by how cheesable it is.
            double cheesedAimValue = aimValue * Math.Pow(attributes.CheeseFactor, 3);
            aimValue = double.Lerp(cheesedAimValue, aimValue, SpecialFunctions.Erf(20 / (Math.Sqrt(2) * (double)deviation)));

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
            if (attributes.ApproachRate > 10.33)
                approachRateFactor = 0.3 * (attributes.ApproachRate - 10.33);

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
                speedValue *= 1.0 + 0.04 * (12.0 - attributes.ApproachRate);
            }

            double speedHighDeviationMultiplier = calculateSpeedHighDeviationNerf(attributes);
            speedValue *= speedHighDeviationMultiplier;

            // Calculate accuracy assuming the worst case scenario
            double relevantTotalDiff = totalHits - attributes.SpeedNoteCount;
            double relevantCountGreat = Math.Max(0, countGreat - relevantTotalDiff);
            double relevantCountOk = Math.Max(0, countOk - Math.Max(0, relevantTotalDiff - countGreat));
            double relevantCountMeh = Math.Max(0, countMeh - Math.Max(0, relevantTotalDiff - countGreat - countOk));
            double relevantAccuracy = attributes.SpeedNoteCount == 0 ? 0 : (relevantCountGreat * 6.0 + relevantCountOk * 2.0 + relevantCountMeh) / (attributes.SpeedNoteCount * 6.0);

            // An effective hit window is created based on the speed SR. The higher the speed difficulty, the shorter the hit window.
            // For example, a speed SR of 4.0 leads to an effective hit window of 20ms, which is OD 10.
            double effectiveHitWindow = Math.Sqrt(20 * 80 / attributes.SpeedDifficulty);

            // Find the proportion of 300s on speed notes assuming the hit window was the effective hit window.
            double effectiveAccuracy = SpecialFunctions.Erf(effectiveHitWindow / (double)speedDeviation);

            // OD 11.11 SS stays the same.
            speedValue *= 1 + 557 / 4860.0;

            // Scale speed value by normalized accuracy.
            speedValue *= Math.Pow(effectiveAccuracy, 2);

            return speedValue;
        }

        private double computeAccuracyValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (score.Mods.Any(h => h is OsuModRelax) || deviation == null)
                return 0.0;

            double accuracyValue = 120 * Math.Pow(7.5 / (double)deviation, 2);

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

            // OD 11.11 SS stays the same. No accuracy scaling.
            flashlightValue *= 1 + 119 / 4050.0;

            return flashlightValue;
        }

        /// <summary>
        /// Estimates player's deviation on speed notes.
        /// Treats all speed notes as hit circles.
        /// </summary>
        private double calculateSpeedDeviation(OsuDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return double.PositiveInfinity;

            double hitWindow300 = attributes.GreatHitWindow;
            double hitWindow100 = attributes.OkHitWindow;
            double hitWindow50 = attributes.MehHitWindow;

            // Calculate accuracy assuming the worst case scenario
            double speedNoteCount = attributes.SpeedNoteCount;
            double relevantTotalDiff = totalHits - attributes.SpeedNoteCount;
            double relevantCountGreat = Math.Max(0, countGreat - relevantTotalDiff);
            double relevantCountOk = Math.Max(0, countOk - Math.Max(0, relevantTotalDiff - countGreat));
            double relevantCountMeh = Math.Max(0, countMeh - Math.Max(0, relevantTotalDiff - countGreat - countOk));
            double relevantCountMiss = Math.Max(0, countMiss - Math.Max(0, relevantTotalDiff - countGreat - countOk - countMeh));

            // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s,
            // compute the deviation on circles.
            if (relevantCountGreat > 0)
            {
                // The probability that a player hits a circle is unknown, but we can estimate it to be
                // the number of greats on circles divided by the number of circles, and then add one
                // to the number of circles as a bias correction.
                double greatProbabilityCircle = relevantCountGreat / (speedNoteCount - relevantCountMiss - relevantCountMeh + 1.0);

                // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
                // Begin with the normal distribution first.
                double deviationOnCircles = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilityCircle));

                // Then compute the variance for 50s.
                double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

                // Find the total deviation.
                deviationOnCircles = Math.Sqrt(((relevantCountGreat + relevantCountOk) * Math.Pow(deviationOnCircles, 2) + relevantCountMeh * mehVariance) / (relevantCountGreat + relevantCountOk + relevantCountMeh));

                return deviationOnCircles;
            }

            return double.PositiveInfinity;
        }

        /// <summary>
        /// Estimates the player's tap deviation based on the OD, given number of greats, oks, mehs and misses,
        /// assuming the player's mean hit error is 0. The estimation is consistent in that two SS scores on the same map with the same settings
        /// will always return the same deviation. Misses are ignored because they are usually due to misaiming.
        /// This method actually gives an upper bound for deviation; i.e. we can be 99% confident that deviation is below this value.
        /// This is so long maps can be less harshly nerfed.
        /// Greats and oks are assumed to follow a normal distribution, whereas mehs are assumed to follow a uniform distribution.
        /// </summary>
        private double? calculateDeviation(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return null;

            double hitWindow300 = attributes.GreatHitWindow;
            double hitWindow100 = attributes.OkHitWindow;
            double hitWindow50 = attributes.MehHitWindow;
            const double z = 2.32634787404; // 99% critical value for the normal distribution (one-tailed).

            if (score.Mods.Any(m => m is OsuModClassic))
            {
                int circleCount = attributes.HitCircleCount;
                int missCountCircles = Math.Min(countMiss, circleCount);
                int mehCountCircles = Math.Min(countMeh, circleCount - missCountCircles);
                int okCountCircles = Math.Min(countOk, circleCount - missCountCircles - mehCountCircles);
                int greatCountCircles = Math.Max(0, circleCount - missCountCircles - mehCountCircles - okCountCircles);

                // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s,
                // compute the deviation on circles.
                if (greatCountCircles > 0)
                {
                    double n = circleCount - missCountCircles - mehCountCircles;

                    // Proportion of greats hit on circles, ignoring misses and 50s.
                    double p = greatCountCircles / n;

                    // We can be 99% confident that p is at least this value.
                    double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

                    // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
                    // Begin with 300s and 100s first. Ignoring 50s, we can be 99% confident that the deviation is not higher than:
                    double deviationOnCircles = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(pLowerBound));
                    double adjustFor100 = Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviationOnCircles, 2)) / (deviationOnCircles * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviationOnCircles)));

                    deviationOnCircles *= Math.Sqrt(1 - adjustFor100);

                    // Value deviation approach as greatCount approaches 0
                    double limitValue = hitWindow100 / Math.Sqrt(3);

                    // If precision is not enough to compute true deviation - use limit value
                    if (pLowerBound == 0 || adjustFor100 >= 1 || deviation > limitValue)
                        deviationOnCircles = limitValue;

                    // Then compute the variance for 50s.
                    double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

                    // Find the total deviation.
                    deviationOnCircles = Math.Sqrt(((greatCountCircles + okCountCircles) * Math.Pow(deviationOnCircles, 2) + mehCountCircles * mehVariance) / (greatCountCircles + okCountCircles + mehCountCircles));

                    return deviationOnCircles;
                }

                // If there are more non-300s than there are circles, compute the deviation on sliders instead.
                // Here, all that matters is whether or not the slider was missed, since it is impossible
                // to get a 100 or 50 on a slider by mis-tapping it.
                int sliderCount = attributes.SliderCount;
                int missCountSliders = Math.Min(sliderCount, countMiss - missCountCircles);
                int greatCountSliders = sliderCount - missCountSliders;

                // We only get here if nothing was hit. In this case, there is no estimate for deviation.
                // Note that this is never negative, so checking if this is only equal to 0 makes sense.
                if (greatCountSliders == 0)
                {
                    return null;
                }

                double greatProbabilitySlider = greatCountSliders / (sliderCount + 1.0);
                double deviationOnSliders = hitWindow50 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilitySlider));

                return deviationOnSliders;
            }
            else
            {
                double n = countGreat + countOk;

                // Proportion of greats hit on circles, ignoring misses and 50s.
                double p = countGreat / n;

                // We can be 99% confident that p is at least this value.
                double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

                // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
                // Begin with 300s and 100s first. Ignoring 50s, we can be 99% confident that the deviation is not higher than:
                double deviation = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(pLowerBound));
                double adjustFor100 = Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviation, 2)) / (deviation * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviation)));

                deviation *= Math.Sqrt(1 - adjustFor100);

                // Value deviation approach as greatCount approaches 0
                double limitValue = hitWindow100 / Math.Sqrt(3);

                // If precision is not enough to compute true deviation - use limit value
                if (pLowerBound == 0 || adjustFor100 >= 1 || deviation > limitValue)
                    deviation = limitValue;

                // Then compute the variance for 50s.
                double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

                // Find the total deviation.
                deviation = Math.Sqrt(((countGreat + countOk) * Math.Pow(deviation, 2) + countMeh * mehVariance) / (countGreat + countOk + countMeh));

                return deviation;
            }
        }

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
