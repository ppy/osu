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
    public abstract class StrainSkill : ProtoStrainSkill
    {
        protected StrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        public sealed override void Process(DifficultyHitObject current)
        {
            Push(current.StartTime, () => StrainValueAt(current), time => startNewSectionFrom(time, current));
        }

        /// <summary>
        /// Returns the strain value at <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
        protected abstract double StrainValueAt(DifficultyHitObject current);

        /// <summary>
        /// Sets the initial strain level for a new section.
        /// </summary>
        /// <param name="time">The beginning of the new section in milliseconds.</param>
        /// <param name="current">The current hit object.</param>
        private void startNewSectionFrom(double time, DifficultyHitObject current)
        {
            // The maximum strain of the new section is not zero by default
            // This means we need to capture the strain level at the beginning of the new section, and use that as the initial peak level.
            CurrentSectionPeak = CalculateInitialStrain(time, current);
        }

        /// <summary>
        /// Retrieves the peak strain at a point in time.
        /// </summary>
        /// <param name="time">The time to retrieve the peak strain at.</param>
        /// <param name="current">The current hit object.</param>
        /// <returns>The peak strain.</returns>
        protected abstract double CalculateInitialStrain(double time, DifficultyHitObject current);
    }
}
