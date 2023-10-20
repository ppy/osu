// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class StrainDecaySkill : StrainSkill
    {
        /// <summary>
        /// Strain values are multiplied by this number for the given skill. Used to balance the value of different skills between each other.
        /// </summary>
        protected abstract double SkillMultiplier { get; }

        /// <summary>
        /// Determines how quickly strain decays for the given skill.
        /// For example a value of 0.15 indicates that strain decays to 15% of its original value in one second.
        /// </summary>
        protected abstract double StrainDecayBase { get; }

        /// <summary>
        /// The current strain level.
        /// </summary>
        protected double CurrentStrain { get; private set; }

        protected StrainDecaySkill(Mod[] mods)
            : base(mods)
        {
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => CurrentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= strainDecay(current.DeltaTime);
            CurrentStrain += StrainValueOf(current) * SkillMultiplier;

            return CurrentStrain;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected abstract double StrainValueOf(DifficultyHitObject current);

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);
    }
}
