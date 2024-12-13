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
        public readonly bool MonoColour;

        private double skillMultiplier => 1.1;
        private double strainDecayBase => 0.4;

        private double currentStrain;

        /// <summary>
        /// Creates a <see cref="Stamina"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        /// <param name="monoColour">Reads when Stamina is from a single coloured pattern.</param>
        public Stamina(Mod[] mods, bool monoColour)
            : base(mods)
        {
            MonoColour = monoColour;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += StaminaEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;

            // Safely prevents previous strains from shifting as new notes are added.
            var currentObject = current as TaikoDifficultyHitObject;
            int index = currentObject?.Colour.MonoStreak?.HitObjects.IndexOf(currentObject) ?? 0;

            if (MonoColour)
                return currentStrain / (1 + Math.Exp(-(index - 10) / 2.0));

            return currentStrain;
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => MonoColour ? 0 : currentStrain * strainDecay(time - current.Previous(0).StartTime);
    }
}
