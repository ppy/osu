// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the stamina coefficient of taiko difficulty.
    /// </summary>
    public class Stamina : StrainSkill
    {
        public override double SkillMultiplier => 1.1;
        protected override double StrainDecayBase => 0.4;

        private readonly bool singleColourStamina;

        /// <summary>
        /// Creates a <see cref="Stamina"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        /// <param name="singleColourStamina">Reads when Stamina is from a single coloured pattern.</param>
        public Stamina(Mod[] mods, bool singleColourStamina)
            : base(mods)
        {
            this.singleColourStamina = singleColourStamina;
        }

        protected override double StrainValueOf(DifficultyHitObject current) => StaminaEvaluator.EvaluateDifficultyOf(current);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);
            CurrentStrain += StrainValueOf(current) * SkillMultiplier;

            // Safely prevents previous strains from shifting as new notes are added.
            var currentObject = current as TaikoDifficultyHitObject;
            int index = currentObject?.Colour.MonoStreak?.HitObjects.IndexOf(currentObject) ?? 0;

            if (singleColourStamina)
                return CurrentStrain / (1 + Math.Exp(-(index - 10) / 2.0));

            return CurrentStrain;
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => singleColourStamina ? 0 : CurrentStrain * StrainDecay(time - current.Previous(0).StartTime);
    }
}
