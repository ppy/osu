// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        /// The current strain level.
        /// </summary>
        protected StrainDecaySkill(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);
            CurrentStrain += StrainValueOf(current);

            return CurrentStrain;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected abstract double StrainValueOf(DifficultyHitObject current);
    }
}
