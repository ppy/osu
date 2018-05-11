// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        private readonly int countHitCircles;
        private readonly int beatmapMaxCombo;

        private Mod[] mods;
        private double realApproachRate;
        private double accuracy;
        private int scoreMaxCombo;
        private int count300;
        private int count100;
        private int count50;
        private int countMiss;

        public OsuPerformanceCalculator(Ruleset ruleset, IBeatmap beatmap, Score score)
            : base(ruleset, beatmap, score)
        {
            countHitCircles = Beatmap.HitObjects.Count(h => h is HitCircle);

            beatmapMaxCombo = Beatmap.HitObjects.Count();
            beatmapMaxCombo += Beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count) + 1;
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            count300 = Convert.ToInt32(Score.Statistics[HitResult.Great]);
            count100 = Convert.ToInt32(Score.Statistics[HitResult.Good]);
            count50 = Convert.ToInt32(Score.Statistics[HitResult.Meh]);
            countMiss = Convert.ToInt32(Score.Statistics[HitResult.Miss]);

            // Don't count scores made with supposedly unranked mods
            if (mods.Any(m => !m.Ranked))
                return 0;

            // Todo: In the future we should apply changes to PreEmpt/AR at an OsuHitObject/BaseDifficulty level, but this is done
            // locally for now as doing so would modify animations and other things unexpectedly
            // DO NOT MODIFY THIS
            double ar = Beatmap.BeatmapInfo.BaseDifficulty.ApproachRate;
            if (mods.Any(m => m is OsuModHardRock))
                ar = Math.Min(10, ar * 1.4);
            if (mods.Any(m => m is OsuModEasy))
                ar = Math.Max(0, ar / 2);
            double preEmpt = BeatmapDifficulty.DifficultyRange(ar, 1800, 1200, 450);
            realApproachRate = preEmpt > 1200 ? (1800 - preEmpt) / 120 : (1200 - preEmpt) / 150 + 5;

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 1.12f; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= 0.90f;

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 0.95f;

            double aimValue = computeAimValue();
            double speedValue = computeSpeedValue();
            double accuracyValue = computeAccuracyValue();
            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1f) +
                    Math.Pow(speedValue, 1.1f) +
                    Math.Pow(accuracyValue, 1.1f), 1.0f / 1.1f
                ) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Speed", speedValue);
                categoryRatings.Add("Accuracy", accuracyValue);
            }

            return totalValue;
        }

        private double computeAimValue()
        {
            double aimValue = Math.Pow(5.0f * Math.Max(1.0f, Attributes["Aim"] / 0.0675f) - 4.0f, 3.0f) / 100000.0f;

            // Longer maps are worth more
            double lengthBonus = 0.95f + 0.4f * Math.Min(1.0f, totalHits / 2000.0f) +
                (totalHits > 2000 ? Math.Log10(totalHits / 2000.0f) * 0.5f : 0.0f);

            aimValue *= lengthBonus;

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            aimValue *= Math.Pow(0.97f, countMiss);

            // Combo scaling
            if (beatmapMaxCombo > 0)
                aimValue *= Math.Min(Math.Pow(scoreMaxCombo, 0.8f) / Math.Pow(beatmapMaxCombo, 0.8f), 1.0f);

            double approachRateFactor = 1.0f;
            if (realApproachRate > 10.33f)
                approachRateFactor += 0.45f * (realApproachRate - 10.33f);
            else if (realApproachRate < 8.0f)
            {
                // HD is worth more with lower ar!
                if (mods.Any(h => h is OsuModHidden))
                    approachRateFactor += 0.02f * (8.0f - realApproachRate);
                else
                    approachRateFactor += 0.01f * (8.0f - realApproachRate);
            }

            aimValue *= approachRateFactor;

            if (mods.Any(h => h is OsuModHidden))
                aimValue *= 1.18f;

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply length bonus again if flashlight is on simply because it becomes a lot harder on longer maps.
                aimValue *= 1.45f * lengthBonus;
            }

            // Scale the aim value with accuracy _slightly_
            aimValue *= 0.5f + accuracy / 2.0f;
            // It is important to also consider accuracy difficulty when doing that
            aimValue *= 0.98f + Math.Pow(Beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 2) / 2500;

            return aimValue;
        }

        private double computeSpeedValue()
        {
            double speedValue = Math.Pow(5.0f * Math.Max(1.0f, Attributes["Speed"] / 0.0675f) - 4.0f, 3.0f) / 100000.0f;

            // Longer maps are worth more
            speedValue *= 0.95f + 0.4f * Math.Min(1.0f, totalHits / 2000.0f) +
                (totalHits > 2000 ? Math.Log10(totalHits / 2000.0f) * 0.5f : 0.0f);

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            speedValue *= Math.Pow(0.97f, countMiss);

            // Combo scaling
            if (beatmapMaxCombo > 0)
                speedValue *= Math.Min(Math.Pow(scoreMaxCombo, 0.8f) / Math.Pow(beatmapMaxCombo, 0.8f), 1.0f);

            // Scale the speed value with accuracy _slightly_
            speedValue *= 0.5f + accuracy / 2.0f;
            // It is important to also consider accuracy difficulty when doing that
            speedValue *= 0.98f + Math.Pow(Beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 2) / 2500;

            return speedValue;
        }

        private double computeAccuracyValue()
        {
            // This percentage only considers HitCircles of any value - in this part of the calculation we focus on hitting the timing hit window
            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = countHitCircles;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((count300 - (totalHits - amountHitObjectsWithAccuracy)) * 6 + count100 * 2 + count50) / (amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points
            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Pow(1.52163f, Beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty) * Math.Pow(betterAccuracyPercentage, 24) * 2.83f;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accuracyValue *= Math.Min(1.15f, Math.Pow(amountHitObjectsWithAccuracy / 1000.0f, 0.3f));

            if (mods.Any(m => m is OsuModHidden))
                accuracyValue *= 1.02f;
            if (mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02f;

            return accuracyValue;
        }

        private double totalHits => count300 + count100 + count50 + countMiss;
        private double totalSuccessfulHits => count300 + count100 + count50;
    }
}
