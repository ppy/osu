// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class StaminaEvaluator
    {
        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this finger.
        /// </summary>
        /// <param name="interval">The interval between the current and previous note hit using the same finger.</param>
        private static double speedBonus(double interval)
        {
            // Interval is capped at a very small value to prevent infinite values.
            interval = Math.Max(interval, 1);

            return 30 / interval;
        }

        /// <summary>
        /// Determines the number of fingers available to hit the current <see cref="TaikoDifficultyHitObject"/>.
        /// Any mono notes that is more than 300ms apart from a colour change will be considered to have more than 2
        /// fingers available, since players can hit the same key with multiple fingers.
        /// </summary>
        private static int availableFingersFor(TaikoDifficultyHitObject hitObject)
        {
            DifficultyHitObject? previousColourChange = hitObject.Colour.PreviousColourChange;
            DifficultyHitObject? nextColourChange = hitObject.Colour.NextColourChange;

            if (previousColourChange != null && hitObject.StartTime - previousColourChange.StartTime < 300)
            {
                return 2;
            }

            if (nextColourChange != null && nextColourChange.StartTime - hitObject.StartTime < 300)
            {
                return 2;
            }

            return 4;
        }

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

            // Find the previous hit object hit by the current finger, which is n notes prior, n being the number of
            // available fingers.
            TaikoDifficultyHitObject taikoCurrent = (TaikoDifficultyHitObject)current;
            TaikoDifficultyHitObject? keyPrevious = taikoCurrent.PreviousMono(availableFingersFor(taikoCurrent) - 1);

            if (keyPrevious == null)
            {
                // There is no previous hit object hit by the current finger
                return 0.0;
            }

            double objectStrain = 0.5; // Add a base strain to all objects
            objectStrain += speedBonus(taikoCurrent.StartTime - keyPrevious.StartTime);
            return objectStrain;
        }
    }
}
