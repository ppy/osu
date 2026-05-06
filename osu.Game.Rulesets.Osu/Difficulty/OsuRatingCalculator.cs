// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuRatingCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        private readonly int totalHits;
        private readonly double overallDifficulty;

        public OsuRatingCalculator(int totalHits, double overallDifficulty)
        {
            this.totalHits = totalHits;
            this.overallDifficulty = overallDifficulty;
        }

        public double ComputeAimRating(double aimDifficultyValue)
        {
            double aimRating = Math.Pow(aimDifficultyValue, 0.65) * 0.01555;

            double ratingMultiplier = 1.0;

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return aimRating * Math.Cbrt(ratingMultiplier);
        }

        public double ComputeSpeedRating(double speedDifficultyValue)
        {
            return CalculateDifficultyRating(speedDifficultyValue);
        }

        public double ComputeReadingRating(double readingDifficultyValue)
        {
            double readingRating = CalculateDifficultyRating(readingDifficultyValue);

            double ratingMultiplier = 1.0;

            ratingMultiplier *= 0.75 + Math.Pow(Math.Max(0, overallDifficulty), 2.2) / 800;

            return readingRating * Math.Cbrt(ratingMultiplier);
        }

        public double ComputeFlashlightRating(double flashlightDifficultyValue)
        {
            double flashlightRating = CalculateDifficultyRating(flashlightDifficultyValue);

            double ratingMultiplier = 1.0;

            // Account for shorter maps having a higher ratio of 0 combo/100 combo flashlight radius.
            ratingMultiplier *= 0.7 + 0.1 * Math.Min(1.0, totalHits / 200.0) +
                                (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return flashlightRating * Math.Sqrt(ratingMultiplier);
        }

        public static double CalculateDifficultyRating(double difficultyValue) => Math.Sqrt(difficultyValue) * difficulty_multiplier;
    }
}
