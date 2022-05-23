// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
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

        /// <summary>
        /// Stamina of each individual keys, calculated based on repetition speed.
        /// </summary>
        private readonly SingleKeyStamina[] keyStamina =
        {
            new SingleKeyStamina(),
            new SingleKeyStamina(),
            new SingleKeyStamina(),
            new SingleKeyStamina()
        };

        /// <summary>
        /// Current index to <see cref="keyStamina" /> for a don hit.
        /// </summary>
        private int donIndex = 1;

        /// <summary>
        /// Current index to <see cref="keyStamina" /> for a kat hit.
        /// </summary>
        private int katIndex = 3;

        /// <summary>
        /// Creates a <see cref="Stamina"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        public Stamina(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Get the next <see cref="SingleKeyStamina"/> to use for the given <see cref="TaikoDifficultyHitObject"/>.
        /// </summary>
        /// <param name="current">The current <see cref="TaikoDifficultyHitObject"/>.</param>
        private SingleKeyStamina getNextSingleKeyStamina(TaikoDifficultyHitObject current)
        {
            // Alternate key for the same color.
            if (current.HitType == HitType.Centre)
            {
                donIndex = donIndex == 0 ? 1 : 0;
                return keyStamina[donIndex];
            }

            katIndex = katIndex == 2 ? 3 : 2;
            return keyStamina[katIndex];
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (!(current.BaseObject is Hit))
            {
                return 0.0;
            }

            TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)current;
            return getNextSingleKeyStamina(hitObject).StrainValueOf(hitObject);
        }
    }
}
