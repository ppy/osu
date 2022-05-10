// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    class SingleKeyStamina
    {
        private double previousHitTime = -1;

        private double strainValueOf(DifficultyHitObject current)
        {
            if (previousHitTime == -1)
            {
                previousHitTime = current.StartTime;
                return 0;
            }
            else
            {
                double objectStrain = 0.5;
                objectStrain += speedBonus(current.StartTime - previousHitTime);
                previousHitTime = current.StartTime;
                return objectStrain;
            }
        }

        public double StrainValueAt(DifficultyHitObject current)
        {
            return strainValueOf(current);
        }

        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this key.
        /// </summary>
        /// <param name="notePairDuration">The duration between the current and previous note hit using the same key.</param>
        private double speedBonus(double notePairDuration)
        {
            return 175 / Math.Pow(notePairDuration + 100, 1);
        }
    }

    /// <summary>
    /// Calculates the stamina coefficient of taiko difficulty.
    /// </summary>
    /// <remarks>
    /// The reference play style chosen uses two hands, with full alternating (the hand changes after every hit).
    /// </remarks>
    public class Stamina : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        private SingleKeyStamina[] keyStamina = new SingleKeyStamina[4]
        {
            new SingleKeyStamina(),
            new SingleKeyStamina(),
            new SingleKeyStamina(),
            new SingleKeyStamina()
        };

        private int donIndex = 1;
        private int katIndex = 3;

        /// <summary>
        /// Creates a <see cref="Stamina"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        public Stamina(Mod[] mods)
            : base(mods)
        {
        }

        private SingleKeyStamina getNextSingleKeyStamina(TaikoDifficultyHitObject current)
        {
            if (current.HitType == HitType.Centre)
            {
                donIndex = donIndex == 0 ? 1 : 0;
                return keyStamina[donIndex];
            }
            else
            {
                katIndex = katIndex == 2 ? 3 : 2;
                return keyStamina[katIndex];
            }
        }

        private double sigmoid(double val, double center, double width)
        {
            return Math.Tanh(Math.E * -(val - center) / width);
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (!(current.BaseObject is Hit))
            {
                return 0.0;
            }

            TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)current;
            double objectStrain = getNextSingleKeyStamina(hitObject).StrainValueAt(hitObject);

            return objectStrain;
        }
    }
}
