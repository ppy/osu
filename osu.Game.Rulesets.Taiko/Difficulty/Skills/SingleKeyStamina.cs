// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Stamina of a single key, calculated based on repetition speed.
    /// </summary>
    public class SingleKeyStamina
    {
        private double? previousHitTime;

        /// <summary>
        /// Similar to <see cref="StrainDecaySkill.StrainValueOf"/>
        /// </summary>
        public double StrainValueOf(DifficultyHitObject current)
        {
            if (previousHitTime == null)
            {
                previousHitTime = current.StartTime;
                return 0;
            }

            double objectStrain = 0.5;
            objectStrain += speedBonus(current.StartTime - previousHitTime.Value);
            previousHitTime = current.StartTime;
            return objectStrain;
        }

        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this key.
        /// </summary>
        /// <param name="notePairDuration">The duration between the current and previous note hit using the same key.</param>
        private double speedBonus(double notePairDuration)
        {
            return 175 / (notePairDuration + 100);
        }
    }
}
