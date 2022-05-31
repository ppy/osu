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
            // TODO: This could result in potential performance issue where it has to check the colour of a large amount
            //       of objects due to previous objects being mono of the other colour. A potential fix for this would be
            //       to store two separate lists of previous objects, one for each colour.
            TaikoDifficultyHitObject taikoCurrent = (TaikoDifficultyHitObject)current;
            TaikoDifficultyHitObject previous = taikoCurrent;
            int monoNoteInterval = 2; // The amount of same-colour notes to go back
            double currentKeyInterval = 0; // Interval of the current key being pressed
            do
            {
                previous = (TaikoDifficultyHitObject)previous.Previous(1);
                if (previous == null) return 0; // No previous (The note is the first press of the current key)
                if (previous.BaseObject is Hit && previous.HitType == taikoCurrent.HitType)
                {
                    --monoNoteInterval;
                }
                currentKeyInterval += previous.DeltaTime;

            } while (monoNoteInterval > 0);

            double objectStrain = 0.5;
            objectStrain += speedBonus(currentKeyInterval);
            return objectStrain;
        }
    }
}
