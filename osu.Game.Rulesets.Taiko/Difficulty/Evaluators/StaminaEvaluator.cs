// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public static class StaminaEvaluator
    {
        /// <summary>
        /// Evaluates the minimum mechanical stamina required to play the current object. This is calculated using the
        /// maximum possible interval between two hits using the same key, by alternating available fingers for each colour.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is not Hit)
            {
                return 0.0;
            }

            TaikoDifficultyHitObject taikoCurrent = (TaikoDifficultyHitObject)current;

            // Add a base strain of 0.5 to all objects
            double objectStrain = 0.5 + speedBonus(taikoCurrent) + monoSpeedBonus(taikoCurrent);

            return objectStrain;
        }

        /// <summary>
        /// Applies a speed bonus dependent on the time since the object before last.
        /// </summary>
        private static double speedBonus(TaikoDifficultyHitObject current)
        {
            TaikoDifficultyHitObject? previous = current.Previous(1) as TaikoDifficultyHitObject;

            if (previous == null)
                return 0.0;

            // Interval is capped at a very small value to prevent infinite values.
            double interval = Math.Max(current.StartTime - previous.StartTime, 1);

            return 10 / interval;
        }

        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this finger.
        /// </summary>
        private static double monoSpeedBonus(TaikoDifficultyHitObject current)
        {
            // Find the previous hit object hit by the current finger, which is n notes prior, n being the number of available fingers.
            TaikoDifficultyHitObject? previousMono = current.PreviousMono(availableFingersFor(current) - 1);

            if (previousMono == null)
                return 0.0;

            // Interval is capped at a very small value to prevent infinite values.
            double interval = Math.Max(current.StartTime - previousMono.StartTime, 1);

            return 20 / interval;
        }

        /// <summary>
        /// Determines the number of fingers available to hit the current <see cref="TaikoDifficultyHitObject"/>.
        /// Any mono notes that is more than 300ms apart from a colour change will be considered to have more than 2
        /// fingers available, since players can hit the same key with multiple fingers.
        /// </summary>
        private static int availableFingersFor(TaikoDifficultyHitObject hitObject)
        {
            DifficultyHitObject? previousColourChange = hitObject.ColourData.PreviousColourChange;
            DifficultyHitObject? nextColourChange = hitObject.ColourData.NextColourChange;

            if (previousColourChange != null && hitObject.StartTime - previousColourChange.StartTime < 300)
            {
                return 2;
            }

            if (nextColourChange != null && nextColourChange.StartTime - hitObject.StartTime < 300)
            {
                return 2;
            }

            return 8;
        }
    }
}
