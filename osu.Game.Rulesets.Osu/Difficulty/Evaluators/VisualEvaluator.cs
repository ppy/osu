// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class VisualEvaluator
    {
        /// <summary>
        /// Evaluates the difficulty of reading the current object, based on:
        /// <list type="bullet">
        /// <item><description>note density of the current object,</description></item>
        /// <item><description>overlapping factor of the current object,</description></item>
        /// <item><description>the preempt time of the current object,</description></item>
        /// <item><description>the velocity of the current object if it's a slider,</description></item>
        /// <item><description>past objects' velocity if they are sliders,</description></item>
        /// <item><description>and whether the Hidden mod is enabled.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool isHiddenMod)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuObj = (OsuHitObject)osuCurrObj.BaseObject;

            // Exclude overlapping objects that can be tapped at once.
            if (osuCurrObj.IsOverlapping(true))
                return 0;

            // Evaluate note density and overlapping factor.
            double noteDensity = 1;
            double overlappingFactor = 0;
            var nextObj = (OsuDifficultyHitObject)current.Next(0);

            for (int i = 0; nextObj != null; nextObj = (OsuDifficultyHitObject)current.Next(++i))
            {
                var osuNextObj = (OsuHitObject)nextObj.BaseObject;

                if (osuNextObj is Spinner)
                    continue;

                double actualDeltaTime = Math.Max(0, nextObj.StartTime - current.EndTime);

                if (actualDeltaTime > osuObj.TimePreempt)
                    break;

                noteDensity += 1 - actualDeltaTime / osuObj.TimePreempt;

                // Penalize objects that are too close to the object in both distance
                // and delta time to prevent stream maps from being overweighted.
                overlappingFactor += Math.Max(0, 1 - (osuObj.StackedPosition - osuNextObj.StackedEndPosition).Length / (3 * osuObj.Radius))
                                    * 7.5 / (1 + Math.Exp(0.15 * (Math.Max(actualDeltaTime, 25) - 75)));
            }

            // Start with base density and give global bonus for Hidden.
            // Add density caps for sanity.
            double strain;

            if (isHiddenMod)
                strain = Math.Min(25, Math.Pow(noteDensity, 2.25));
            else
                strain = Math.Min(20, Math.Pow(noteDensity, 2));

            // Bonus based on how visible the object is.
            for (int i = 0; i < Math.Min(current.Index, 10); i++)
            {
                var previous = (OsuDifficultyHitObject)current.Previous(i);

                // Exclude overlapping objects that can be tapped at once.
                if (previous.BaseObject is Spinner || previous.IsOverlapping(true))
                    continue;

                double actualDeltaTime = current.StartTime - previous.EndTime;

                // Do not consider objects that don't fall under time preempt.
                if (actualDeltaTime > osuObj.TimePreempt)
                    break;

                strain += (1 - osuCurrObj.OpacityAt(previous.BaseObject.StartTime, isHiddenMod)) / 4;
            }

            // Scale the value with overlapping factor.
            strain /= 10 * (1 + overlappingFactor);

            if (osuObj.TimePreempt < 400)
            {
                // Give bonus for AR higher than 10.33.
                strain += Math.Pow(400 - osuObj.TimePreempt, 1.3) / 100;
            }

            if (current.BaseObject is Slider slider)
            {
                double scalingFactor = 50 / slider.Radius;

                // Reward sliders based on velocity.
                strain +=
                    // Avoid overbuffing extremely fast sliders.
                    Math.Min(6, slider.Velocity * 1.5) *
                    // Scale with distance travelled to avoid overbuffing fast sliders with short distance.
                    Math.Min(1, osuCurrObj.TravelDistance / scalingFactor / 125);

                double cumulativeStrainTime = 0;

                // Reward for velocity changes based on last few sliders.
                for (int i = 0; i < Math.Min(current.Index, 4); i++)
                {
                    var previous = (OsuDifficultyHitObject)current.Previous(i);

                    cumulativeStrainTime += previous.StrainTime;

                    if (previous.BaseObject is Slider prevSlider && !previous.IsOverlapping(true))
                    {
                        strain +=
                            // Avoid overbuffing extremely fast velocity changes.
                            Math.Min(10, 2.5 * Math.Abs(slider.Velocity - prevSlider.Velocity)) *
                            // Scale with distance travelled to avoid overbuffing fast sliders with short distance.
                            Math.Min(1, previous.TravelDistance / scalingFactor / 100) *
                            // Scale with cumulative strain time to avoid overbuffing past sliders.
                            Math.Min(1, 300 / cumulativeStrainTime);
                    }
                }
            }

            return strain;
        }
    }
}