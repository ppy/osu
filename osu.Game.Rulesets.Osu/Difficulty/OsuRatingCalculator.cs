// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuRatingCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        private readonly Mod[] mods;
        private readonly int totalHits;
        private readonly double approachRate;
        private readonly double overallDifficulty;
        private readonly double mechanicalDifficultyRating;
        private readonly double sliderFactor;

        public OsuRatingCalculator(Mod[] mods, int totalHits, double approachRate, double overallDifficulty, double mechanicalDifficultyRating, double sliderFactor)
        {
            this.mods = mods;
            this.totalHits = totalHits;
            this.approachRate = approachRate;
            this.overallDifficulty = overallDifficulty;
            this.mechanicalDifficultyRating = mechanicalDifficultyRating;
            this.sliderFactor = sliderFactor;
        }

        public double ComputeAimRating(double aimDifficultyValue)
        {
            if (mods.Any(m => m is OsuModAutopilot))
                return 0;

            double aimRating = CalculateDifficultyRating(aimDifficultyValue);

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

            double approachRateLengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                             (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);

            double approachRateFactor = 0.0;
            if (approachRate > 10.33)
                approachRateFactor = 0.3 * (approachRate - 10.33);
            else if (approachRate < 8.0)
                approachRateFactor = 0.05 * (8.0 - approachRate);

            if (mods.Any(h => h is OsuModRelax))
                approachRateFactor = 0.0;

            ratingMultiplier += approachRateFactor * approachRateLengthBonus; // Buff for longer maps with high AR.

            if (mods.Any(m => m is OsuModHidden))
            {
                double visibilityFactor = calculateAimVisibilityFactor(approachRate);
                ratingMultiplier += CalculateVisibilityBonus(mods, approachRate, visibilityFactor, sliderFactor);
            }

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return aimRating * Math.Cbrt(ratingMultiplier);
        }

        public double ComputeSpeedRating(double speedDifficultyValue)
        {
            if (mods.Any(m => m is OsuModRelax))
                return 0;

            double speedRating = CalculateDifficultyRating(speedDifficultyValue);

            if (mods.Any(m => m is OsuModAutopilot))
                speedRating *= 0.5;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                // reduce speed rating because of the speed distance scaling, with maximum reduction being 0.7x
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                speedRating *= 1.0 - magnetisedStrength * 0.3;
            }

            double ratingMultiplier = 1.0;

            double approachRateLengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                             (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);

            double approachRateFactor = 0.0;
            if (approachRate > 10.33)
                approachRateFactor = 0.3 * (approachRate - 10.33);

            if (mods.Any(m => m is OsuModAutopilot))
                approachRateFactor = 0.0;

            ratingMultiplier += approachRateFactor * approachRateLengthBonus; // Buff for longer maps with high AR.

            if (mods.Any(m => m is OsuModHidden))
            {
                double visibilityFactor = calculateSpeedVisibilityFactor(approachRate);
                ratingMultiplier += CalculateVisibilityBonus(mods, approachRate, visibilityFactor);
            }

            ratingMultiplier *= 0.95 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 750;

            return speedRating * Math.Cbrt(ratingMultiplier);
        }

        public double ComputeFlashlightRating(double flashlightDifficultyValue)
        {
            if (!mods.Any(m => m is OsuModFlashlight))
                return 0;

            double flashlightRating = CalculateDifficultyRating(flashlightDifficultyValue);

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

        private double calculateAimVisibilityFactor(double approachRate)
        {
            const double ar_factor_end_point = 11.5;

            double mechanicalDifficultyFactor = DifficultyCalculationUtils.ReverseLerp(mechanicalDifficultyRating, 5, 10);
            double arFactorStartingPoint = double.Lerp(9, 10.33, mechanicalDifficultyFactor);

            return DifficultyCalculationUtils.ReverseLerp(approachRate, ar_factor_end_point, arFactorStartingPoint);
        }

        private double calculateSpeedVisibilityFactor(double approachRate)
        {
            const double ar_factor_end_point = 11.5;

            double mechanicalDifficultyFactor = DifficultyCalculationUtils.ReverseLerp(mechanicalDifficultyRating, 5, 10);
            double arFactorStartingPoint = double.Lerp(10, 10.33, mechanicalDifficultyFactor);

            return DifficultyCalculationUtils.ReverseLerp(approachRate, ar_factor_end_point, arFactorStartingPoint);
        }

        /// <summary>
        /// Calculates a visibility bonus that is applicable to Hidden and Traceable.
        /// </summary>
        public static double CalculateVisibilityBonus(Mod[] mods, double approachRate, double visibilityFactor = 1, double sliderFactor = 1)
        {
            // NOTE: TC's effect is only noticeable in performance calculations until lazer mods are accounted for server-side.
            bool isAlwaysPartiallyVisible = mods.OfType<OsuModHidden>().Any(m => m.OnlyFadeApproachCircles.Value) || mods.OfType<OsuModTraceable>().Any();

            // Start from normal curve, rewarding lower AR up to AR7
            // TC forcefully requires a lower reading bonus for now as it's post-applied in PP which makes it multiplicative with the regular AR bonuses
            // This means it has an advantage over HD, so we decrease the multiplier to compensate
            // This should be removed once we're able to apply TC bonuses in SR (depends on real-time difficulty calculations being possible)
            double readingBonus = (isAlwaysPartiallyVisible ? 0.025 : 0.04) * (12.0 - Math.Max(approachRate, 7));

            readingBonus *= visibilityFactor;

            // We want to reward slideraim on low AR less
            double sliderVisibilityFactor = Math.Pow(sliderFactor, 3);

            // For AR up to 0 - reduce reward for very low ARs when object is visible
            if (approachRate < 7)
                readingBonus += (isAlwaysPartiallyVisible ? 0.02 : 0.045) * (7.0 - Math.Max(approachRate, 0)) * sliderVisibilityFactor;

            // Starting from AR0 - cap values so they won't grow to infinity
            if (approachRate < 0)
                readingBonus += (isAlwaysPartiallyVisible ? 0.01 : 0.1) * (1 - Math.Pow(1.5, approachRate)) * sliderVisibilityFactor;

            return readingBonus;
        }

        public static double CalculateDifficultyRating(double difficultyValue) => Math.Sqrt(difficultyValue) * difficulty_multiplier;
    }
}
