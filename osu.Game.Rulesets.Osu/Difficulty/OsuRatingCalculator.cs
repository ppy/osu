// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuRatingCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        private readonly Mod[] mods;
        private readonly int totalHits;
        private readonly double overallDifficulty;
        private readonly double sliderFactor;

        public OsuRatingCalculator(Mod[] mods, int totalHits, double overallDifficulty, double sliderFactor)
        {
            this.mods = mods;
            this.totalHits = totalHits;
            this.overallDifficulty = overallDifficulty;
            this.sliderFactor = sliderFactor;
        }

        public double ComputeAimRating(double aimDifficultyValue)
        {
            if (mods.Any(m => m is OsuModAutopilot))
                return 0;

            double aimRating = calculateDifficultyRating(aimDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
                aimRating = Math.Pow(aimRating, 0.8);

            if (mods.Any(m => m is OsuModRelax))
                aimRating *= 0.9;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                aimRating *= 1.0 - magnetisedStrength;
            }

            double ratingMultiplier = 1.0;

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return aimRating * Math.Cbrt(ratingMultiplier);
        }

        public double ComputeSpeedRating(double speedDifficultyValue)
        {
            if (mods.Any(m => m is OsuModRelax))
                return 0;

            double speedRating = calculateDifficultyRating(speedDifficultyValue);

            if (mods.Any(m => m is OsuModAutopilot))
                speedRating *= 0.5;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                // reduce speed rating because of the speed distance scaling, with maximum reduction being 0.7x
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                speedRating *= 1.0 - magnetisedStrength * 0.3;
            }

            double ratingMultiplier = 1.0;

            ratingMultiplier *= 0.95 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 750;

            return speedRating * Math.Cbrt(ratingMultiplier);
        }

        public double ComputeReadingRating(double readingDifficultyValue)
        {
            double readingRating = calculateDifficultyRating(readingDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
                readingRating = Math.Pow(readingRating, 0.8);

            if (mods.Any(m => m is OsuModRelax))
                readingRating *= 0.7;
            else if (mods.Any(m => m is OsuModAutopilot))
                readingRating *= 0.4;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                readingRating *= 1.0 - magnetisedStrength;
            }

            double ratingMultiplier = 1.0;

            // It is important to also consider accuracy difficulty when doing that.
            ratingMultiplier *= 0.75 + Math.Pow(Math.Max(0, overallDifficulty), 2.2) / 800;

            return readingRating * Math.Sqrt(ratingMultiplier);
        }

        public double ComputeFlashlightRating(double flashlightDifficultyValue)
        {
            if (!mods.Any(m => m is OsuModFlashlight))
                return 0;

            double flashlightRating = calculateDifficultyRating(flashlightDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
                flashlightRating = Math.Pow(flashlightRating, 0.8);

            if (mods.Any(m => m is OsuModRelax))
                flashlightRating *= 0.7;
            else if (mods.Any(m => m is OsuModAutopilot))
                flashlightRating *= 0.4;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                flashlightRating *= 1.0 - magnetisedStrength;
            }

            if (mods.Any(m => m is OsuModDeflate))
            {
                float deflateInitialScale = mods.OfType<OsuModDeflate>().First().StartScale.Value;
                flashlightRating *= Math.Clamp(DifficultyCalculationUtils.ReverseLerp(deflateInitialScale, 11, 1), 0.1, 1);
            }

            double ratingMultiplier = 1.0;

            // Account for shorter maps having a higher ratio of 0 combo/100 combo flashlight radius.
            ratingMultiplier *= 0.7 + 0.1 * Math.Min(1.0, totalHits / 200.0) +
                                (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return flashlightRating * Math.Sqrt(ratingMultiplier);
        }

        /// <summary>
        /// Calculates a visibility bonus that is applicable to Hidden and Traceable.
        /// </summary>
        public static double CalculateVisibilityBonus(double approachRate, double visibilityFactor = 1, double sliderFactor = 1)
        {
            // Start from normal curve, rewarding lower AR up to AR7
            double readingBonus = 0.04 * (12.0 - Math.Max(approachRate, 7));

            readingBonus *= visibilityFactor;

            // We want to reward slideraim on low AR less
            double sliderVisibilityFactor = Math.Pow(sliderFactor, 3);

            // For AR up to 0 - reduce reward for very low ARs when object is visible
            if (approachRate < 7)
                readingBonus += 0.03 * (7.0 - Math.Max(approachRate, 0))  * sliderVisibilityFactor;

            // Starting from AR0 - cap values so they won't grow to infinity
            if (approachRate < 0)
                readingBonus += 0.075 * (1 - Math.Pow(1.5, approachRate)) * sliderVisibilityFactor;

            return readingBonus;
        }

        private static double calculateDifficultyRating(double difficultyValue) => Math.Sqrt(difficultyValue) * difficulty_multiplier;
    }
}
