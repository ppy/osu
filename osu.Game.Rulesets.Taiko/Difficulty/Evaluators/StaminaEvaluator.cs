// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class StaminaEvaluator
    {
        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this key.
        /// </summary>
        /// <param name="notePairDuration">The duration between the current and previous note hit using the same key.</param>
        private static double speedBonus(double notePairDuration)
        {
            return 175 / (notePairDuration + 100);
        }

        /// <summary>
        /// Evaluates the minimum mechanical stamina required to play the current object. This is calculated using the
        /// maximum  possible interval between two hits using the same key, by alternating 2 keys for each colour.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (!(current.BaseObject is Hit))
            {
                return 0.0;
            }

            // Find the previous hit object hit by the current key, which is two notes of the same colour prior.
            TaikoDifficultyHitObject taikoCurrent = (TaikoDifficultyHitObject)current;
            TaikoDifficultyHitObject keyPrevious = taikoCurrent.PreviousMono(1);
            if (keyPrevious == null)
            {
                // There is no previous hit object hit by the current key
                return 0.0;
            }

            double objectStrain = 0.5;
            objectStrain += speedBonus(taikoCurrent.StartTime - keyPrevious.StartTime);
            return objectStrain;
        }
    }
}
