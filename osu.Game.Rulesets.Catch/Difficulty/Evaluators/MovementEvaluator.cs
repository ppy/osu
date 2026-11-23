// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Rulesets.Catch.Difficulty.Evaluators
{
    public static class MovementEvaluator
    {
        private const double direction_change_bonus = 21.0;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;
            var catchLast = (CatchDifficultyHitObject?)current.Previous();
            var catchLastLast = (CatchDifficultyHitObject?)current.Previous(1);

            // In catch, clockrate adjustments do not only affect the timings of hitobjects,
            // but also the speed of the player's catcher, which has an impact on difficulty
            double clockRate = current.ClockRate;

            double weightedStrainTime = catchCurrent.StrainTime + 16 / clockRate;

            double distanceAddition = (DiffUtils.Pow(Math.Abs(catchCurrent.DistanceMoved), 1.3) / 510);
            double sqrtStrain = Math.Sqrt(weightedStrainTime);

            double edgeDashBonus = 0;

            // Direction change bonus.
            if (Math.Abs(catchCurrent.DistanceMoved) > 0.1)
            {
                if (catchLast != null && Math.Abs(catchLast.DistanceMoved) > 0.1 && catchCurrent.MovementDirection != catchLast.MovementDirection)
                {
                    double bonusFactor = Math.Min(50, Math.Abs(catchCurrent.DistanceMoved)) / 50;
                    double antiflowFactor = Math.Max(Math.Min(70, Math.Abs(catchLast.DistanceMoved)) / 70, 0.38);

                    distanceAddition += direction_change_bonus / Math.Sqrt(catchLast.StrainTime + 16 / clockRate) * bonusFactor * antiflowFactor
                                        * Math.Max(1 - DiffUtils.Pow(weightedStrainTime / 1000, 3), 0);
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 12.5 * Math.Min(Math.Abs(catchCurrent.DistanceMoved), CatchDifficultyHitObject.NORMALIZED_HALF_CATCHER_WIDTH * 2)
                                    / (CatchDifficultyHitObject.NORMALIZED_HALF_CATCHER_WIDTH * 6) / sqrtStrain;
            }

            // Repetitive spacing penalty: it is harder to catch irregular distances, especially in case of non-hyperdashes which require one to "miss the rhythm on purpose".
            // This feature may be treated as a reading penalty: even if the mechanical difficulty is the same, executing it is easier in case of repetitive spacings.
            // Penalty is given based on the ratio between the last normalised spacing and the previous normalised spacings.
            if (catchLast != null)
            {
                double repetitiveSpacingWeight = 0;

                const double repetitive_spacing_cutoff = 0.15;
                const double full_repetitive_spacing_penalty_cutoff = 0.05;
                const double repetitive_timing_cutoff = 0.1;
                const double repetitive_spacing_power = 0.9;

                var currentFlow = catchCurrent.Flow;
                var lastFlow = catchLast.Flow;

                // Penalty is applied only to the direction changes (under a convention that we exclude DistanceMoved == 0).
                if (currentFlow.IsFirst(catchCurrent))
                {
                    for (int i = 0; i < Math.Min(lastFlow.Index, 8); i++)
                    {
                        var prevFlow = lastFlow.Previous(i);

                        if (prevFlow != null && prevFlow.DistanceMoved != 0)
                        {
                            double lastSpacing = Math.Abs(lastFlow.DistanceMoved / lastFlow.StrainTime);
                            double prevSpacing = Math.Abs(prevFlow.DistanceMoved / prevFlow.StrainTime);

                            double relativeDifference = Math.Abs(lastSpacing / prevSpacing - 1);
                            relativeDifference = Math.Max(0, relativeDifference - full_repetitive_spacing_penalty_cutoff); // Handle inaccuracies in repetitive patterns.

                            // Scaling by time relative time difference: additional penalty for maintaining the rhythm.
                            double relativeTimeDifference = Math.Abs(lastFlow.StrainTime / prevFlow.StrainTime - 1);
                            relativeTimeDifference = 5.0 * Math.Clamp(relativeTimeDifference, 0, repetitive_timing_cutoff); // We apply only 50% of the penalty for different time interval than the current one.

                            if (relativeDifference <= repetitive_spacing_cutoff)
                                repetitiveSpacingWeight += (1 - relativeDifference / repetitive_spacing_cutoff)
                                                        * (1 - relativeTimeDifference)
                                                        * DiffUtils.Pow(repetitive_spacing_power, i);
                        }
                    }
                }

                double repetitiveSpacingPenalty = catchCurrent.BaseObject.HyperDash ? DiffUtils.Pow(0.96, repetitiveSpacingWeight) : DiffUtils.Pow(0.93, repetitiveSpacingWeight);
                distanceAddition *= repetitiveSpacingPenalty;
            }

            // Linear spacing nerf.
            int linearSpacingCount = 0;

            for (int i = 0; i < Math.Min(current.Index, 10); i++)
            {
                var catchPrevObj = (CatchDifficultyHitObject)catchCurrent.Previous(i)!;

                // Only same direction movements matter as they do not take any additional inputs.
                if (catchCurrent.MovementDirection != catchPrevObj.MovementDirection || catchCurrent.DistanceMoved == 0 || catchPrevObj.DistanceMoved == 0)
                    break;

                double currentSpacing = Math.Abs(catchCurrent.DistanceMoved / catchCurrent.StrainTime);
                double prevSpacing = Math.Abs(catchPrevObj.DistanceMoved / catchPrevObj.StrainTime);

                double relativeDifference = Math.Abs(currentSpacing / prevSpacing - 1);

                if (relativeDifference > 0.05)
                    break;

                linearSpacingCount++;
            }

            distanceAddition *= DiffUtils.Pow(0.7, linearSpacingCount);

            // Bonus for edge dashes.
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 5.7;

                distanceAddition *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20)
                                                        * DiffUtils.Pow((Math.Min(catchCurrent.StrainTime * clockRate, 265) / 265), 1.5); // Edge Dashes are easier at lower ms values
            }

            // There is an edge case where horizontal back and forth sliders create "buzz" patterns which are repeated "movements" with a distance lower than
            // the platter's width but high enough to be considered a movement due to the absolute_player_positioning_error and NORMALIZED_HALF_CATCHER_WIDTH offsets
            // We are detecting this exact scenario. The first back and forth is counted but all subsequent ones are nullified.
            // To achieve that, we need to store the exact distances between consecutive objects.
            if (catchLast != null && catchLastLast != null
                                  && Math.Abs(catchCurrent.Distance) <= CatchDifficultyHitObject.NORMALIZED_HALF_CATCHER_WIDTH * 2
                                  && Precision.AlmostEquals(catchCurrent.Distance, -catchLast.Distance)
                                  && Precision.AlmostEquals(catchLast.Distance, -catchLastLast.Distance)
                                  && Precision.AlmostEquals(catchCurrent.StrainTime, catchLast.StrainTime)
                                  && Precision.AlmostEquals(catchLast.StrainTime, catchLastLast.StrainTime))
                distanceAddition = 0;

            return distanceAddition / weightedStrainTime;
        }
    }
}
