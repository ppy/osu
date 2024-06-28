// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public const double PERFORMANCE_BASE_MULTIPLIER = 1.14; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things.

        private double accuracy;
        private int scoreMaxCombo;

        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private int countSliderBreaks;
        private int countSliderEndsDropped;

        private double effectiveMissCount;

        private bool usingSliderAccuracy;
        private double hitWindow300, hitWindow100, hitWindow50;
        private double deviation, speedDeviation;
        private double deviationARadjust;

        public OsuPerformanceCalculator()
            : base(new OsuRuleset())
        {
        }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var osuAttributes = (OsuDifficultyAttributes)attributes;

            accuracy = score.Accuracy;
            scoreMaxCombo = score.MaxCombo;
            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);
            countSliderBreaks = score.Statistics.GetValueOrDefault(HitResult.LargeTickMiss);

            usingSliderAccuracy = !score.Mods.Any(h => h is OsuModClassic cl && cl.NoSliderHeadAccuracy.Value);

            if (usingSliderAccuracy)
            {
                effectiveMissCount = countMiss;
                countSliderEndsDropped = osuAttributes.SliderCount - score.Statistics.GetValueOrDefault(HitResult.SliderTailHit);
            }
            else
            {
                effectiveMissCount = calculateEffectiveMissCount(osuAttributes);
                countSliderEndsDropped = Math.Min(countOk + countMeh + countMiss, attributes.MaxCombo - scoreMaxCombo);
            }

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

            var track = new TrackVirtual(1);
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            double clockRate = track.Rate;

            hitWindow300 = 80 - 6 * osuAttributes.OverallDifficulty;
            hitWindow100 = (140 - 8 * ((80 - hitWindow300 * clockRate) / 6)) / clockRate;
            hitWindow50 = (200 - 10 * ((80 - hitWindow300 * clockRate) / 6)) / clockRate;

            // Bonus for low AR to account for the fact that it's more difficult to get low UR on low AR
            deviationARadjust = 0.475 + 0.7 / (1.0 + Math.Pow(1.73, 7.9 - osuAttributes.ApproachRate));

            deviation = calculateDeviation(score, osuAttributes) * deviationARadjust;
            speedDeviation = calculateSpeedDeviation(score, osuAttributes) * deviationARadjust;

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
                Deviation = deviation / deviationARadjust,
                SpeedDeviation = speedDeviation / deviationARadjust,
                Total = totalValue
            };
        }

        private double computeAimValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (deviation == double.PositiveInfinity)
                return 0.0;

            double aimValue = Math.Pow(5.0 * Math.Max(1.0, attributes.AimDifficulty / 0.0675) - 4.0, 3.0) / 100000.0;

            // Rake tapping nerf
            aimValue = adjustPerformanceWithUR(aimValue, deviation);

            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            aimValue *= lengthBonus;

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            if (effectiveMissCount > 0)
                aimValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), effectiveMissCount);

            aimValue *= getComboScalingFactor(attributes);

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

            // We assume 15% of sliders in a map are difficult since there's no way to tell from the performance calculator.
            double estimateDifficultSliders = attributes.SliderCount * 0.15;

            if (attributes.SliderCount > 0)
            {
                double estimateSliderEndsDropped = Math.Clamp(countSliderEndsDropped + countSliderBreaks * 2, 0, estimateDifficultSliders);
                double sliderNerfFactor = (1 - attributes.SliderFactor) * Math.Pow(1 - estimateSliderEndsDropped / estimateDifficultSliders, 3) + attributes.SliderFactor;
                aimValue *= sliderNerfFactor;
            }

            // Scale the aim value with  deviation
            aimValue *= SpecialFunctions.Erf(30 / (Math.Sqrt(2) * deviation));
            aimValue *= 0.98 + Math.Pow(100.0 / 9, 2) / 2500; // OD 11 SS stays the same.

            return aimValue;
        }

        private double computeSpeedValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (score.Mods.Any(h => h is OsuModRelax) || speedDeviation == double.PositiveInfinity)
                return 0.0;

            double speedValue = Math.Pow(5.0 * Math.Max(1.0, attributes.SpeedDifficulty / 0.0675) - 4.0, 3.0) / 100000.0;

            // Rake tapping nerf
            speedValue = adjustSpeedWithUR(speedValue, speedDeviation / deviationARadjust);

            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            speedValue *= lengthBonus;

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            if (effectiveMissCount > 0)
                speedValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), Math.Pow(effectiveMissCount, .875));

            speedValue *= getComboScalingFactor(attributes);

            double approachRateFactor = 0.0;
            if (attributes.ApproachRate > 10.33)
                approachRateFactor = 0.3 * (attributes.ApproachRate - 10.33);

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

            // Scale the speed value with speed deviation
            speedValue *= SpecialFunctions.Erf(20 / (Math.Sqrt(2) * speedDeviation));
            speedValue *= 0.95 + Math.Pow(100.0 / 9, 2) / 750; // OD 11 SS stays the same.

            return speedValue;
        }

        private double computeAccuracyValue(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (deviation == double.PositiveInfinity || score.Mods.Any(h => h is OsuModRelax) || deviation == double.PositiveInfinity)
                return 0.0;

            int amountHitObjectsWithAccuracy = attributes.HitCircleCount;
            if (usingSliderAccuracy) amountHitObjectsWithAccuracy += attributes.SliderCount;

            double liveLengthBonus = Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 1000.0, 0.3));
            double threshold = 1000 * Math.Pow(1.15, 1 / 0.3); // Number of objects until length bonus caps.

            // Some fancy stuff to make curve similar to live
            double scaling = Math.Sqrt(2) * Math.Log(1.52163) * SpecialFunctions.ErfInv(1 / (1 + 1 / Math.Min(amountHitObjectsWithAccuracy, threshold))) / 6;

            // Accuracy pp formula that's roughly the same as live.
            double accuracyValue = 2.83 * Math.Pow(1.52163, 40.0 / 3) * liveLengthBonus * Math.Exp(-scaling * deviation);

            // Punish very low amount of hits additionally to prevent big pp values right at the start of the map
            if (amountHitObjectsWithAccuracy < 30)
                accuracyValue *= Math.Sqrt((double)amountHitObjectsWithAccuracy / 30);

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
            if (!score.Mods.Any(h => h is OsuModFlashlight) || deviation == double.PositiveInfinity)
                return 0.0;

            double flashlightValue = Math.Pow(attributes.FlashlightDifficulty, 2.0) * 25.0;

            // Rake tapping nerf
            flashlightValue = adjustPerformanceWithUR(flashlightValue, deviation);

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            if (effectiveMissCount > 0)
                flashlightValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), Math.Pow(effectiveMissCount, .875));

            flashlightValue *= getComboScalingFactor(attributes);

            // Account for shorter maps having a higher ratio of 0 combo/100 combo flashlight radius.
            flashlightValue *= 0.7 + 0.1 * Math.Min(1.0, totalHits / 200.0) +
                               (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            // Scale the flashlight value with deviation
            flashlightValue *= SpecialFunctions.Erf(50 / (Math.Sqrt(2) * deviation));
            flashlightValue *= 0.98 + Math.Pow(100.0 / 9, 2) / 2500;  // OD 11 SS stays the same.

            return flashlightValue;
        }

        private double calculateEffectiveMissCount(OsuDifficultyAttributes attributes)
        {
            // Guess the number of misses + slider breaks from combo
            double comboBasedMissCount = 0.0;

            if (attributes.SliderCount > 0)
            {
                double fullComboThreshold = attributes.MaxCombo - 0.1 * attributes.SliderCount;
                if (scoreMaxCombo < fullComboThreshold)
                    comboBasedMissCount = fullComboThreshold / Math.Max(1.0, scoreMaxCombo);
            }

            // Clamp miss count to maximum amount of possible breaks
            comboBasedMissCount = Math.Min(comboBasedMissCount, countOk + countMeh + countMiss);

            return Math.Max(countMiss, comboBasedMissCount);
        }

        /// <summary>
        /// Estimates the player's tap deviation based on the OD, number of circles and sliders, and number of 300s, 100s, 50s, and misses,
        /// assuming the player's mean hit error is 0. The estimation is consistent in that two SS scores on the same map with the same settings
        /// will always return the same deviation. Sliders are treated as circles with a 50 hit window. Misses are ignored because they are usually due to misaiming.
        /// 300s and 100s are assumed to follow a normal distribution, whereas 50s are assumed to follow a uniform distribution.
        /// </summary>
        private double calculateDeviation(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return double.PositiveInfinity;

            int accuracyObjectCount = attributes.HitCircleCount;
            if (usingSliderAccuracy) accuracyObjectCount += attributes.SliderCount;

            int missCountCircles = Math.Min(countMiss, accuracyObjectCount);
            int mehCountCircles = Math.Min(countMeh, accuracyObjectCount - missCountCircles);
            int okCountCircles = Math.Min(countOk, accuracyObjectCount - missCountCircles - mehCountCircles);
            int greatCountCircles = Math.Max(0, accuracyObjectCount - missCountCircles - mehCountCircles - okCountCircles);

            if (greatCountCircles + okCountCircles + mehCountCircles <= 0)
                return double.PositiveInfinity;

            double deviation = double.PositiveInfinity;

            // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s,
            // compute the deviation on circles.
            if (accuracyObjectCount > 0)
            {
                //// The probability that a player hits a circle is unknown, but we can estimate it to be
                //// the number of greats on circles divided by the number of circles, and then add one
                //// to the number of circles as a bias correction.
                double n = Math.Max(1, accuracyObjectCount - missCountCircles - mehCountCircles);
                const double z = 2.32634787404; // 99% critical value for the normal distribution (one-tailed).

                // Proportion of greats hit on circles, ignoring misses and 50s.
                double p = greatCountCircles / n;

                // We can be 99% confident that p is at least this value.
                double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

                // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
                // Begin with 300s and 100s first. Ignoring 50s, we can be 99% confident that the deviation is not higher than:
                deviation = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(pLowerBound));

                double randomValue = Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviation, 2))
                    / (deviation * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviation)));

                deviation *= Math.Sqrt(1 - randomValue);

                // If precision is not enough - use limit value
                if (pLowerBound == 0 || randomValue >= 1)
                    deviation = hitWindow100 / Math.Sqrt(3);

                // Then compute the variance for 50s.
                double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

                // Find the total deviation.
                deviation = Math.Sqrt(((greatCountCircles + okCountCircles) * Math.Pow(deviation, 2) + mehCountCircles * mehVariance) / (greatCountCircles + okCountCircles + mehCountCircles));

                // Adjust by 0.9 to account for the fact that it's higher bound UR value
                deviation *= 0.9;
            }

            if (usingSliderAccuracy) return deviation;

            // If there are more non-300s than there are circles, compute the deviation on sliders instead.
            // Here, all that matters is whether or not the slider was missed, since it is impossible
            // to get a 100 or 50 on a slider by mis-tapping it.
            double deviationOnSliders = double.PositiveInfinity;
            int sliderCount = attributes.SliderCount;
            int missCountSliders = Math.Min(sliderCount, countMiss - missCountCircles);
            int greatCountSliders = sliderCount - missCountSliders;

            // We only get here if nothing was hit. In this case, there is no estimate for deviation.
            // Note that this is never negative, so checking if this is only equal to 0 makes sense.
            if (greatCountSliders > 0)
            {
                double greatProbabilitySlider = greatCountSliders / (sliderCount + 1.0);
                deviationOnSliders = hitWindow50 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilitySlider));
            }

            return Math.Min(deviation, deviationOnSliders);
        }

        /// <summary>
        /// Does the same as <see cref="calculateDeviation"/>, but only for notes and inaccuracies that are relevant to speed difficulty.
        /// Treats all difficult speed notes as circles, so this method can sometimes return a lower deviation than <see cref="calculateDeviation"/>.
        /// This is fine though, since this method is only used to scale speed pp.
        /// </summary>
        private double calculateSpeedDeviation(ScoreInfo score, OsuDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return double.PositiveInfinity;

            // Calculate accuracy assuming the worst case scenario
            double speedNoteCount = attributes.SpeedNoteCount;

            double relevantTotalDiff = totalHits - speedNoteCount;

            double relevantCountGreat = Math.Max(0, countGreat - relevantTotalDiff);
            double relevantCountOk = Math.Max(0, countOk - Math.Max(0, relevantTotalDiff - countGreat));
            double relevantCountMeh = Math.Max(0, countMeh - Math.Max(0, relevantTotalDiff - countGreat - countOk));
            double relevantCountMiss = Math.Max(0, countMiss - Math.Max(0, relevantTotalDiff - countGreat - countOk - countMeh));

            if (usingSliderAccuracy)
            {
                double greatsRatio = relevantCountGreat / speedNoteCount;
                double mistapRatio = 1 - greatsRatio;

                // Assume sliders are 2 times easier to acc than streams
                double hitcircleRatio = attributes.HitCircleCount / (attributes.HitCircleCount + attributes.SliderCount / 2.0);
                mistapRatio *= hitcircleRatio;

                // This can't get higher than total value
                double adjustedGreatsRatio = Math.Min(1 - mistapRatio, (double)countGreat / totalHits);

                double mistapsMultiplier = (greatsRatio == 1) ? 0 : (1 - adjustedGreatsRatio) / (1 - greatsRatio);

                relevantCountGreat = speedNoteCount * adjustedGreatsRatio;
                relevantCountOk *= mistapsMultiplier;
                relevantCountMeh *= mistapsMultiplier;
                relevantCountMiss *= mistapsMultiplier;
            }

            if (relevantCountGreat + relevantCountOk + relevantCountMeh <= 0)
                return double.PositiveInfinity;

            double n = Math.Max(1, speedNoteCount - relevantCountMiss - relevantCountMeh);
            const double z = 2.32634787404; // 99% critical value for the normal distribution (one-tailed).

            // Proportion of greats hit on circles, ignoring misses and 50s.
            double p = relevantCountGreat / n;

            // We can be 99% confident that p is at least this value.
            double pLowerBound = (n * p + z * z / 2) / (n + z * z) - z / (n + z * z) * Math.Sqrt(n * p * (1 - p) + z * z / 4);

            // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
            // Begin with 300s and 100s first. Ignoring 50s, we can be 99% confident that the deviation is not higher than:
            double deviation = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(pLowerBound));

            double randomValue = Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviation, 2))
                / (deviation * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviation)));

            deviation *= Math.Sqrt(1 - randomValue);

            // If precision is not enough - use limit value
            if (pLowerBound == 0 || randomValue >= 1)
                deviation = hitWindow100 / Math.Sqrt(3);

            // Then compute the variance for 50s.
            double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

            // Find the total deviation.
            deviation = Math.Sqrt(((relevantCountGreat + relevantCountOk) * Math.Pow(deviation, 2) + relevantCountMeh * mehVariance) / (relevantCountGreat + relevantCountOk + relevantCountMeh));

            // Adjust by 0.9 to account for the fact that it's higher bound UR value
            return deviation * 0.9;
        }

        // https://www.desmos.com/calculator/no1pv5gv5g
        private double adjustSpeedWithUR(double speedValue, double UR)
        {
            // Starting from this pp amount - penalty will be applied
            double abusePoint = 100 + 220 * Math.Pow(20 / UR, 4.5);

            if (speedValue <= abusePoint)
                return speedValue;

            // Descale values to make log curve look correctly
            const double scale = 50;
            speedValue = scale * (Math.Log(speedValue / scale + 1 - abusePoint / scale) + abusePoint / scale);

            return speedValue;
        }

        private double adjustPerformanceWithUR(double performanceValue, double UR)
        {
            // Starting from this pp amount - penalty will be applied
            double abusePoint = 200 + 260 * Math.Pow(20 / UR, 4.5);

            if (performanceValue <= abusePoint)
                return performanceValue;

            // Descale value to make log curve look correctly
            const double scale = 50;
            performanceValue = scale * (Math.Log(performanceValue / scale + 1 - abusePoint / scale) + abusePoint / scale);

            return performanceValue;
        }

        private double getComboScalingFactor(OsuDifficultyAttributes attributes) => attributes.MaxCombo <= 0 ? 1.0 : Math.Min(Math.Pow(scoreMaxCombo, 0.8) / Math.Pow(attributes.MaxCombo, 0.8), 1.0);
        private int totalHits => countGreat + countOk + countMeh + countMiss;
        private int totalSuccessfulHits => countGreat + countOk + countMeh;
    }
}
