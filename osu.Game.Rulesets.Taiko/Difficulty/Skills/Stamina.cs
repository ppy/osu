// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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

        private readonly SingleKeyStamina[] centreKeyStamina =
        {
            new SingleKeyStamina(),
            new SingleKeyStamina()
        };

        private readonly SingleKeyStamina[] rimKeyStamina =
        {
            new SingleKeyStamina(),
            new SingleKeyStamina()
        };

        /// <summary>
        /// Current index into <see cref="centreKeyStamina" /> for a centre hit.
        /// </summary>
        private int centreKeyIndex;

        /// <summary>
        /// Current index into <see cref="rimKeyStamina" /> for a rim hit.
        /// </summary>
        private int rimKeyIndex;

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
                centreKeyIndex = (centreKeyIndex + 1) % 2;
                return centreKeyStamina[centreKeyIndex];
            }

            rimKeyIndex = (rimKeyIndex + 1) % 2;
            return rimKeyStamina[rimKeyIndex];
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
