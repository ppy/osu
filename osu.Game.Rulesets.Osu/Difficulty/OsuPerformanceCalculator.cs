// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public new OsuDifficultyAttributes Attributes => (OsuDifficultyAttributes)base.Attributes;

        private readonly int countHitCircles;
        private readonly int beatmapMaxCombo;

        private Mod[] mods;

        private double accuracy;
        private int scoreMaxCombo;
        private int countGreat;
        private int countGood;
        private int countMeh;
        private int countMiss;

        private const double miss_decay = 0.985;
        private const double combo_weight = 0.5;

        public OsuPerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, ScoreInfo score)
            : base(ruleset, beatmap, score)
        {
            countHitCircles = Beatmap.HitObjects.Count(h => h is HitCircle);

            beatmapMaxCombo = Beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the "headcircle" would be counted twice (once for the slider itself in the line above)
            beatmapMaxCombo += Beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            countGreat = Convert.ToInt32(Score.Statistics[HitResult.Great]);
            countGood = Convert.ToInt32(Score.Statistics[HitResult.Good]);
            countMeh = Convert.ToInt32(Score.Statistics[HitResult.Meh]);
            countMiss = Convert.ToInt32(Score.Statistics[HitResult.Miss]);

            // Don't count scores made with supposedly unranked mods
            if (mods.Any(m => !m.Ranked))
                return 0;

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 1.12f; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= 0.90f;

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 0.95f;

            double aimValue = computeAimValue(categoryRatings);
            double speedValue = computeSpeedValue(categoryRatings);
            double accuracyValue = computeAccuracyValue(categoryRatings);
            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1f) +
                    Math.Pow(speedValue, 1.1f) +
                    Math.Pow(accuracyValue, 1.1f), 1.0f / 1.1f
                ) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", beatmapMaxCombo);
            }

            return totalValue;
        }

        private double interpComboStarRating(IList<double> values, double scoreCombo, double mapCombo)
        {
            if (mapCombo == 0)
            {
                return values.Last();
            }

            double comboRatio = scoreCombo / mapCombo;
            double pos = Math.Min(comboRatio * (values.Count), values.Count);
            int i = (int)pos;

            if (i == values.Count)
            {
                return values.Last();
            }

            if (pos <= 0)
            {
                return 0;
            }

            double ub = values[i];
            double lb = i == 0 ? 0 : values[i - 1];

            double t = pos - i;
            double ret = lb * (1 - t) + ub * t;

            return ret;
        }

        private double interpMissCountStarRating(double sr, IList<double> values, int missCount)
        {
            double increment = Attributes.MissStarRatingIncrement;
            double t;

            if (missCount == 0)
            {
                // zero misses, return SR
                return sr;
            }

            if (missCount < values[0])
            {
                return sr - increment * missCount / values[0];
            }

            for (int i = 0; i < values.Count; ++i)
            {
                if (missCount == values[i])
                {
                    if (i < values.Count - 1 && missCount == values[i + 1])
                    {
                        // if there are duplicates, take the lowest SR that can achieve miss count
                        continue;
                    }

                    return sr - (i + 1) * increment;
                }

                if (i < values.Count - 1 && missCount < values[i + 1])
                {
                    t = (missCount - values[i]) / (values[i + 1] - values[i]);

                    return sr - (i + 1 + t) * increment;
                }
            }

            // more misses than max evaluated, interpolate to zero
            t = (missCount - values.Last()) / (beatmapMaxCombo - values.Last());
            return (sr - values.Count * increment) * (1 - t);
        }

        private double computeAimValue(Dictionary<string, double> categoryRatings = null)
        {
            double aimComboStarRating = interpComboStarRating(Attributes.AimComboStarRatings, scoreMaxCombo, beatmapMaxCombo);
            double aimMissCountStarRating = interpMissCountStarRating(Attributes.AimStrain, Attributes.AimMissCounts, countMiss);
            double rawAim = Math.Pow(aimComboStarRating, combo_weight) * Math.Pow(aimMissCountStarRating, 1 - combo_weight);

            if (mods.Any(m => m is OsuModTouchDevice))
                rawAim = Math.Pow(rawAim, 0.8);

            double aimValue = Math.Pow(5.0f * Math.Max(1.0f, rawAim / 0.0675f) - 4.0f, 3.0f) / 100000.0f;

            // discourage misses
            aimValue *= Math.Pow(miss_decay, countMiss);

            double approachRateFactor = 1.0f;

            if (Attributes.ApproachRate > 10.33f)
                approachRateFactor += 0.3f * (Attributes.ApproachRate - 10.33f);
            else if (Attributes.ApproachRate < 8.0f)
            {
                approachRateFactor += 0.01f * (8.0f - Attributes.ApproachRate);
            }

            aimValue *= approachRateFactor;

            // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
            if (mods.Any(h => h is OsuModHidden))
                aimValue *= 1.0f + 0.04f * (12.0f - Attributes.ApproachRate);

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply object-based bonus for flashlight.
                aimValue *= 1.0f + 0.35f * Math.Min(1.0f, totalHits / 200.0f) +
                            (totalHits > 200
                                ? 0.3f * Math.Min(1.0f, (totalHits - 200) / 300.0f) +
                                  (totalHits > 500 ? (totalHits - 500) / 1200.0f : 0.0f)
                                : 0.0f);
            }

            // Scale the aim value with accuracy _slightly_
            aimValue *= 0.5f + accuracy / 2.0f;
            // It is important to also consider accuracy difficulty when doing that
            aimValue *= 0.98f + Math.Pow(Attributes.OverallDifficulty, 2) / 2500;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Aim Combo Stars", aimComboStarRating);
                categoryRatings.Add("Aim Miss Count Stars", aimMissCountStarRating);
            }

            return aimValue;
        }

        private double computeSpeedValue(Dictionary<string, double> categoryRatings = null)
        {
            double speedComboStarRating = interpComboStarRating(Attributes.SpeedComboStarRatings, scoreMaxCombo, beatmapMaxCombo);
            double speedMissCountStarRating = interpMissCountStarRating(Attributes.SpeedStrain, Attributes.SpeedMissCounts, countMiss);
            double rawSpeed = Math.Pow(speedComboStarRating, combo_weight) * Math.Pow(speedMissCountStarRating, 1 - combo_weight);
            //System.Console.WriteLine($"speed Misses: [ {String.Join(", ", Attributes.SpeedMissCounts)}]");

            double speedValue = Math.Pow(5.0f * Math.Max(1.0f, rawSpeed / 0.0675f) - 4.0f, 3.0f) / 100000.0f;

            // discourage misses
            speedValue *= Math.Pow(miss_decay, countMiss);

            double approachRateFactor = 1.0f;
            if (Attributes.ApproachRate > 10.33f)
                approachRateFactor += 0.3f * (Attributes.ApproachRate - 10.33f);

            speedValue *= approachRateFactor;

            if (mods.Any(m => m is OsuModHidden))
                speedValue *= 1.0f + 0.04f * (12.0f - Attributes.ApproachRate);

            // Scale the speed value with accuracy _slightly_
            speedValue *= 0.02f + accuracy;
            // It is important to also consider accuracy difficulty when doing that
            speedValue *= 0.96f + Math.Pow(Attributes.OverallDifficulty, 2) / 1600;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Speed", speedValue);
                categoryRatings.Add("Speed Combo Stars", speedComboStarRating);
                categoryRatings.Add("Speed Miss Count Stars", speedMissCountStarRating);
            }

            return speedValue;
        }

        private double computeAccuracyValue(Dictionary<string, double> categoryRatings = null)
        {
            // This percentage only considers HitCircles of any value - in this part of the calculation we focus on hitting the timing hit window
            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = countHitCircles;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((countGreat - (totalHits - amountHitObjectsWithAccuracy)) * 6 + countGood * 2 + countMeh) / (amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points
            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Pow(1.52163f, Attributes.OverallDifficulty) * Math.Pow(betterAccuracyPercentage, 24) * 2.83f;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accuracyValue *= Math.Min(1.15f, Math.Pow(amountHitObjectsWithAccuracy / 1000.0f, 0.3f));

            if (mods.Any(m => m is OsuModHidden))
                accuracyValue *= 1.08f;
            if (mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02f;

            categoryRatings?.Add("Accuracy", accuracyValue);

            return accuracyValue;
        }

        private double totalHits => countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulHits => countGreat + countGood + countMeh;
    }
}
