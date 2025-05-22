// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public static class LegacyScoreUtils
    {
        /// <summary>
        /// Calculates the average amount of score per object that is caused by nested judgements such as slider-ticks and spinners.
        /// </summary>
        public static double CalculateNestedScorePerObject(IBeatmap beatmap, int objectCount)
        {
            const double big_tick_score = 30;
            const double small_tick_score = 10;

            var sliders = beatmap.HitObjects.OfType<Slider>().ToArray();

            // 1 for head, 1 for tail
            int amountOfBigTicks = sliders.Length * 2;

            // Add slider repeats
            amountOfBigTicks += sliders.Select(s => s.RepeatCount).Sum();

            int amountOfSmallTicks = sliders.Select(s => s.NestedHitObjects.Count(nho => nho is SliderTick)).Sum();

            double sliderScore = amountOfBigTicks * big_tick_score + amountOfSmallTicks * small_tick_score;

            double spinnerScore = 0;

            foreach (var spinner in beatmap.HitObjects.OfType<Spinner>())
            {
                spinnerScore += calculateSpinnerScore(spinner);
            }

            return (sliderScore + spinnerScore) / objectCount;
        }

        /// <remarks>
        /// Logic borrowed from <see cref="OsuLegacyScoreSimulator.simulateHit"/> for basic score calculations.
        /// </remarks>
        private static double calculateSpinnerScore(Spinner spinner)
        {
            const int spin_score = 100;
            const int bonus_spin_score = 1000;

            // The spinner object applies a lenience because gameplay mechanics differ from osu-stable.
            // We'll redo the calculations to match osu-stable here...
            const double maximum_rotations_per_second = 477.0 / 60;

            // Normally, this value depends on the final overall difficulty. For simplicity, we'll only consider the worst case that maximises bonus score.
            // As we're primarily concerned with computing the maximum theoretical final score,
            // this will have the final effect of slightly underestimating bonus score achieved on stable when converting from score V1.
            const double minimum_rotations_per_second = 3;

            double secondsDuration = spinner.Duration / 1000;

            // The total amount of half spins possible for the entire spinner.
            int totalHalfSpinsPossible = (int)(secondsDuration * maximum_rotations_per_second * 2);
            // The amount of half spins that are required to successfully complete the spinner (i.e. get a 300).
            int halfSpinsRequiredForCompletion = (int)(secondsDuration * minimum_rotations_per_second);
            // To be able to receive bonus points, the spinner must be rotated another 1.5 times.
            int halfSpinsRequiredBeforeBonus = halfSpinsRequiredForCompletion + 3;

            long score = 0;

            int fullSpins = (totalHalfSpinsPossible / 2);

            // Normal spin score
            score += spin_score * fullSpins;

            int bonusSpins = (totalHalfSpinsPossible - halfSpinsRequiredBeforeBonus) / 2;

            // Reduce amount of bonus spins because we want to represent the more average case, rather than the best one.
            bonusSpins = Math.Max(0, bonusSpins - fullSpins / 2);

            score += bonus_spin_score * bonusSpins;

            return score;
        }

        public static int CalculateDifficultyPeppyStars(IBeatmap beatmap)
        {
            int objectCount = beatmap.HitObjects.Count;
            int drainLength = 0;

            if (objectCount > 0)
            {
                int breakLength = beatmap.Breaks.Select(b => (int)Math.Round(b.EndTime) - (int)Math.Round(b.StartTime)).Sum();
                drainLength = ((int)Math.Round(beatmap.HitObjects[^1].StartTime) - (int)Math.Round(beatmap.HitObjects[0].StartTime) - breakLength) / 1000;
            }

            return LegacyRulesetExtensions.CalculateDifficultyPeppyStars(beatmap.Difficulty, objectCount, drainLength);
        }
    }
}
