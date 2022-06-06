// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Stamina of a single key, calculated based on repetition speed.
    /// </summary>
    public class SingleKeyStamina
    {
        private const double StrainDecayBase = 0.4;

        private double CurrentStrain = 0;

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

            // CurrentStrain += strainDecay(current.StartTime - current.Previous(0).StartTime);
            // CurrentStrain += 0.5 + 0.5 * strainDecay(current.StartTime - current.Previous(0).StartTime);
            CurrentStrain += 1;
            CurrentStrain *= ColourEvaluator.EvaluateDifficultyOf(current) * 0.1 + 0.9;
            CurrentStrain *= strainDecay(current.StartTime - previousHitTime.Value);
            previousHitTime = current.StartTime;
            return CurrentStrain;
        }

        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this key.
        /// </summary>
        /// <param name="notePairDuration">The duration between the current and previous note hit using the same key.</param>
        private double strainDecay(double notePairDuration)
        {
            return Math.Pow(StrainDecayBase, notePairDuration / 1000);
            // return 175 / (notePairDuration + 100);
        }
    }
}
